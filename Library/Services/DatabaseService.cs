namespace Library.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Threading.Tasks;
    using Models;

    public static class DatabaseService
    {
        /// <summary>
        /// Save the data retrieved from the api in the database
        /// </summary>
        /// <param name="Countries">List of countries</param>
        /// <param name="progress">Progress report</param>
        /// <returns>Returns a task</returns>
        public static async Task SaveData(List<Country> Countries, IProgress<ProgressReport> progress)
        {
            List<Language> Languages = new List<Language>();
            List<Currency> Currencies = new List<Currency>();
            List<RegionalBloc> RegionalBlocs = new List<RegionalBloc>();

            OrganizeData(Countries, Languages, Currencies, RegionalBlocs);

            if (!Directory.Exists("DB"))
            {
                Directory.CreateDirectory("DB");
            }

            var path = @"DB\countries.sqlite";

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + path))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    await CreateDatabase(command);

                    await DeleteData(command);

                    await InsertData(command, Countries, Languages, Currencies, RegionalBlocs, progress);
                }
            }
        }

        /// <summary>
        /// Get distinct languages, currencies and regional blocs
        /// </summary>
        /// <param name="Countries">List of countries</param>
        /// <param name="Languages">List of languages (empty)</param>
        /// <param name="Currencies">List of currencies (empty)</param>
        /// <param name="RegionalBlocs">List of regional blocs (empty)</param>
        private static void OrganizeData(List<Country> Countries, List<Language> Languages, List<Currency> Currencies, List<RegionalBloc> RegionalBlocs)
        {
            //Get distinct languages
            foreach (var country in Countries)
            {
                if (Languages.Count == 0)
                {
                    foreach (var language in country.Languages)
                    {
                        Languages.Add(language);
                    }
                }
                else
                {
                    foreach (var language in country.Languages)
                    {
                        if (!Languages.Contains(language))
                        {
                            Languages.Add(language);
                        }
                    }
                }
            }

            //Get distinct currencies
            foreach (var country in Countries)
            {
                if (Currencies.Count == 0)
                {
                    foreach (var currency in country.Currencies)
                    {
                        Currencies.Add(currency);
                    }
                }
                else
                {
                    foreach (var currency in country.Currencies)
                    {
                        if (!Currencies.Contains(currency))
                        {
                            Currencies.Add(currency);
                        }
                    }
                }
            }

            //Get distinct regional blocs
            foreach (var country in Countries)
            {
                if (RegionalBlocs.Count == 0)
                {
                    foreach (var regionalBloc in country.RegionalBlocs)
                    {
                        RegionalBlocs.Add(regionalBloc);
                    }
                }
                else
                {
                    foreach (var regionalBloc in country.RegionalBlocs)
                    {
                        if (!RegionalBlocs.Contains(regionalBloc))
                        {
                            RegionalBlocs.Add(regionalBloc);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create the database
        /// </summary>
        /// <param name="command">Sqlite command</param>
        /// <returns>Returns a task</returns>
        private static async Task CreateDatabase(SQLiteCommand command)
        {
            await Task.Run(() =>
            {
                try
                {
                    //Table country
                    command.CommandText = "create table if not exists country(alpha3code varchar(3) primary key, name varchar(100), alpha2code varchar(2), capital varchar(50), region varchar(10), subregion varchar(30), population int, demonym varchar(50), area real, gini real, native_name nvarchar(100), numeric_code varchar(3), flag text, cioc varchar(3))";
                    command.ExecuteNonQueryAsync();

                    //Table top_level_domain
                    command.CommandText = "create table if not exists top_level_domain(id integer primary key autoincrement, top_level_domain varchar(3), country_alpha3code char(3) references country(alpha3code))";
                    command.ExecuteNonQueryAsync();

                    //Table calling_code
                    command.CommandText = "create table if not exists calling_code(id integer primary key autoincrement, calling_codes varchar(5), country_alpha3code char(3) references country(alpha3code))";
                    command.ExecuteNonQueryAsync();

                    //Table alt_spelling
                    command.CommandText = "create table if not exists alt_spelling(id integer primary key autoincrement, alt_spellings nvarchar(100), country_alpha3code char(3) references country(alpha3code))";
                    command.ExecuteNonQueryAsync();

                    //Table lat_lng
                    command.CommandText = "create table if not exists lat_lng(country_alpha3code char(3) primary key references country(alpha3code), lat real, lng real)";
                    command.ExecuteNonQueryAsync();

                    //Table translation
                    command.CommandText = "create table if not exists translation(country_alpha3code char(3) primary key references country(alpha3code), de nvarchar(100), es nvarchar(100), fr nvarchar(100), ja nvarchar(100), it nvarchar(100), br nvarchar(100), pt nvarchar(100), nl nvarchar(100), hr nvarchar(100), fa nvarchar(100))";
                    command.ExecuteNonQueryAsync();

                    //Table country_border
                    command.CommandText = "create table if not exists country_border(country_alpha3code char(3) references country(alpha3code), borders varchar(3), primary key(country_alpha3code, borders))";
                    command.ExecuteNonQueryAsync();

                    //Table country_timezone
                    command.CommandText = "create table if not exists country_timezone(country_alpha3code char(3) references country(alpha3code), timezones varchar(9), primary key(country_alpha3code, timezones))";
                    command.ExecuteNonQueryAsync();

                    //Table language
                    command.CommandText = "create table if not exists language(iso639_1 varchar(5) primary key, iso639_2 varchar(5), name varchar(50), native_name nvarchar(50))";
                    command.ExecuteNonQueryAsync();

                    //Table country_language
                    command.CommandText = "create table if not exists country_language(country_alpha3code char(3) references country(alpha3code), iso639_1_language varchar(5) references language(iso639_1), primary key(country_alpha3code, iso639_1_language))";
                    command.ExecuteNonQueryAsync();

                    //Table currency
                    command.CommandText = "create table if not exists currency(code varchar(5) primary key, name varchar(50), symbol nvarchar(5))";
                    command.ExecuteNonQueryAsync();

                    //Table country_currency
                    command.CommandText = "create table if not exists country_currency(country_alpha3code char(3) references country(alpha3code), code_currency varchar(5) references currency(code), primary key(country_alpha3code, code_currency))";
                    command.ExecuteNonQueryAsync();

                    //Table regional_bloc
                    command.CommandText = "create table if not exists regional_bloc(acronym varchar(10) primary key, name varchar(50))";
                    command.ExecuteNonQueryAsync();

                    //Table country_regional_bloc
                    command.CommandText = "create table if not exists country_regional_bloc(country_alpha3code char(3) references country(alpha3code), acronym_regional_bloc varchar(10) references regional_bloc(acronym), primary key(country_alpha3code, acronym_regional_bloc))";
                    command.ExecuteNonQueryAsync();

                    //Table other_acronym
                    command.CommandText = "create table if not exists other_acronym(id integer primary key autoincrement, other_acronyms varchar(10), acronym_regional_bloc varchar(10) references regional_bloc(acronym))";
                    command.ExecuteNonQueryAsync();

                    //Table other_name
                    command.CommandText = "create table if not exists other_name(id integer primary key autoincrement, other_names nvarchar(100), acronym_regional_bloc varchar(10) references regional_bloc(acronym))";
                    command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    DialogService.ShowMessageBox("Error", e.Message);
                }
            });
        }

        /// <summary>
        /// Delete data from the database
        /// </summary>
        /// <param name="command">Sqlite command</param>
        /// <returns>Returns a task</returns>
        private static async Task DeleteData(SQLiteCommand command)
        {
            await Task.Run(() =>
            {
                try
                {
                    //Delete data from table country_regional_bloc
                    command.CommandText = "delete from country_regional_bloc";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table country_currency
                    command.CommandText = "delete from country_currency";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table country_language
                    command.CommandText = "delete from country_language";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table country_timezone
                    command.CommandText = "delete from country_timezone";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table country_border
                    command.CommandText = "delete from country_border";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table translation
                    command.CommandText = "delete from translation";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table lat_lng
                    command.CommandText = "delete from lat_lng";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table alt_spelling
                    command.CommandText = "delete from alt_spelling";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table calling_code
                    command.CommandText = "delete from calling_code";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table top_level_domain
                    command.CommandText = "delete from top_level_domain";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table country
                    command.CommandText = "delete from country";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table other_name
                    command.CommandText = "delete from other_name";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table other_acronym
                    command.CommandText = "delete from other_acronym";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table regional_bloc
                    command.CommandText = "delete from regional_bloc";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table currency
                    command.CommandText = "delete from currency";
                    command.ExecuteNonQueryAsync();

                    //Delete data from table language
                    command.CommandText = "delete from language";
                    command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    DialogService.ShowMessageBox("Error", e.Message);
                }
            });
        }


        /// <summary>
        /// Insert the data into the database
        /// </summary>
        /// <param name="command">Sqlite command</param>
        /// <param name="Countries">List of countries</param>
        /// <param name="Languages">List of distinct languages</param>
        /// <param name="Currencies">List of distinct currencies</param>
        /// <param name="RegionalBlocs">List of distinct regional blocs</param>
        /// <param name="progress">Progress report</param>
        /// <returns>Returns a task</returns>
        private static async Task InsertData(SQLiteCommand command, List<Country> Countries, List<Language> Languages, List<Currency> Currencies, List<RegionalBloc> RegionalBlocs, IProgress<ProgressReport> progress)
        {
            ProgressReport report = new ProgressReport();
            int count = 0;

            await Task.Run(() =>
            {
                try
                {
                    foreach (var regionalBloc in RegionalBlocs)
                    {
                        //Fill table regional_bloc with data
                        command.CommandText = $"insert into regional_bloc values('{regionalBloc.Acronym}','{regionalBloc.Name}')";
                        command.ExecuteNonQueryAsync();

                        //Fill table other_acronym with data
                        foreach (var otherAcronym in regionalBloc.OtherAcronyms)
                        {
                            command.CommandText = $"insert into other_acronym(other_acronyms,acronym_regional_bloc) values('{otherAcronym}','{regionalBloc.Acronym}')";
                            command.ExecuteNonQueryAsync();
                        }

                        //Fill table other_name with data
                        foreach (var otherName in regionalBloc.OtherNames)
                        {
                            command.CommandText = $"insert into other_name(other_names,acronym_regional_bloc) values('{otherName}','{regionalBloc.Acronym}')";
                            command.ExecuteNonQueryAsync();
                        }
                    }

                    progress.Report(report);

                    //Fill table language with data
                    foreach (var language in Languages)
                    {
                        command.CommandText = $"insert into language values('{language.Iso639_1}','{language.Iso639_2}','{language.Name}','{language.NativeName.Replace("'", "´")}')";
                        command.ExecuteNonQueryAsync();
                    }

                    count += 3;
                    report.PercentageComplete = (count * 100) / 256;
                    progress.Report(report);

                    //Fill table currency with data
                    foreach (var currency in Currencies)
                    {
                        command.CommandText = $"insert into currency values('{currency.Code}','{currency.Name.Replace("'", "´")}','{currency.Symbol}')";
                        command.ExecuteNonQueryAsync();
                    }

                    count += 3;
                    report.PercentageComplete = (count * 100) / 256;
                    progress.Report(report);

                    foreach (var country in Countries)
                    {
                        //Fill table country with data
                        command.CommandText = $"insert into country values('{country.Alpha3Code}','{country.Name.Replace("'", "´")}','{country.Alpha2Code}','{country.Capital.Replace("'", "´")}','{country.Region}','{country.Subregion}',{country.Population},'{country.Demonym}','{country.Area}','{country.Gini}','{country.NativeName.Replace("'", "´")}','{country.NumericCode}','{country.Flag}','{country.Cioc}')";
                        command.ExecuteNonQueryAsync();

                        //Fill table top_level_domain with data
                        foreach (var topLevelDomain in country.TopLevelDomain)
                        {
                            command.CommandText = $"insert into top_level_domain(top_level_domain,country_alpha3code) values('{topLevelDomain}','{country.Alpha3Code}')";
                            command.ExecuteNonQueryAsync();
                        }

                        //Fill table calling_code with data
                        foreach (var callingCode in country.CallingCodes)
                        {
                            command.CommandText = $"insert into calling_code(calling_codes,country_alpha3code) values('{callingCode}','{country.Alpha3Code}')";
                            command.ExecuteNonQueryAsync();
                        }

                        //Fill table alt_spelling with data
                        foreach (var altSpelling in country.AltSpellings)
                        {
                            command.CommandText = $"insert into alt_spelling(alt_spellings,country_alpha3code) values('{altSpelling.Replace("'", "´")}','{country.Alpha3Code}')";
                            command.ExecuteNonQueryAsync();
                        }

                        //Fill table lat_lng with data
                        if (country.Latlng.Count > 0)
                        {
                            command.CommandText = $"insert into lat_lng values('{country.Alpha3Code}','{country.Latlng[0]}','{country.Latlng[1]}')";
                            command.ExecuteNonQueryAsync();
                        }

                        //Fill table translation with data
                        if (country.Translations.It == null)
                        {
                            country.Translations.It = string.Empty;
                        }

                        if (country.Translations.Fr == null)
                        {
                            country.Translations.Fr = string.Empty;
                        }

                        command.CommandText = $"insert into translation values('{country.Alpha3Code}','{country.Translations.De}','{country.Translations.Es}','{country.Translations.Fr.Replace("'", "´")}','{country.Translations.Ja}','{country.Translations.It.Replace("'", "´")}','{country.Translations.Br}','{country.Translations.Pt}','{country.Translations.Nl}','{country.Translations.Hr}','{country.Translations.Fa}')";
                        command.ExecuteNonQueryAsync();

                        //Fill table country_border with data
                        foreach (var border in country.Borders)
                        {
                            command.CommandText = $"insert into country_border values('{country.Alpha3Code}','{border}')";
                            command.ExecuteNonQueryAsync();
                        }

                        //Fill table country_timezone with data
                        foreach (var timeZone in country.Timezones)
                        {
                            command.CommandText = $"insert into country_timezone values('{country.Alpha3Code}','{timeZone}')";
                            command.ExecuteNonQueryAsync();
                        }

                        //Fill table country_language with data
                        foreach (var language in country.Languages)
                        {
                            command.CommandText = $"insert into country_language values('{country.Alpha3Code}','{language.Iso639_1}')";
                            command.ExecuteNonQueryAsync();
                        }

                        //Fill table country_currency with data
                        foreach (var currency in country.Currencies)
                        {
                            command.CommandText = $"insert into country_currency values('{country.Alpha3Code}','{currency.Code}')";
                            command.ExecuteNonQueryAsync();
                        }

                        //Fill table country_regional_bloc with data
                        foreach (var regionalBloc in country.RegionalBlocs)
                        {
                            command.CommandText = $"insert into country_regional_bloc values('{country.Alpha3Code}','{regionalBloc.Acronym}')";
                            command.ExecuteNonQueryAsync();
                        }

                        count++;
                        report.PercentageComplete = (count * 100) / 256;
                        progress.Report(report);
                    }
                }
                catch (Exception e)
                {
                    DialogService.ShowMessageBox("Error", e.Message);
                }
            });
        }

        /// <summary>
        /// Get the data from the database in a list of objects of the type Country
        /// </summary>
        /// <param name="progress">Progress report</param>
        /// <returns>Returns a list of objects of the type Country</returns>
        public async static Task<List<Country>> GetData(IProgress<ProgressReport> progress)
        {
            List<Country> Countries = new List<Country>();

            if (!Directory.Exists("DB"))
            {
                Directory.CreateDirectory("DB");
            }

            var path = @"DB\countries.sqlite";

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + path))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    await CreateDatabase(command);
                }

                Countries = await RetrieveDataFromDatabase(connection, progress);
            }

            return Countries;
        }

        /// <summary>
        /// Retrieve the data from the database
        /// </summary>
        /// <param name="connection">Sqlconnection</param>
        /// <param name="progress">Progress report</param>
        /// <returns>Returns a list of objects of the type Country</returns>
        private async static Task<List<Country>> RetrieveDataFromDatabase(SQLiteConnection connection, IProgress<ProgressReport> progress)
        {
            List<Country> Countries = new List<Country>();
            ProgressReport report = new ProgressReport();

            await Task.Run(() =>
            {
                try
                {
                    using (SQLiteCommand command1 = new SQLiteCommand(connection))
                    {
                        //Get data from the table country
                        command1.CommandText = "select alpha3code, name, alpha2code, capital, region, subregion, population, demonym, area, gini, native_name, numeric_code, flag, cioc from country";

                        using (SQLiteDataReader readerCountry = command1.ExecuteReader())
                        {
                            while (readerCountry.Read())
                            {
                                Country country = new Country
                                {
                                    Name = (string)readerCountry["name"],
                                    Alpha2Code = (string)readerCountry["alpha2code"],
                                    Alpha3Code = (string)readerCountry["alpha3code"],
                                    Capital = (string)readerCountry["capital"],
                                    Region = (string)readerCountry["region"],
                                    Subregion = (string)readerCountry["subregion"],
                                    Population = (int)readerCountry["population"],
                                    Demonym = (string)readerCountry["demonym"],
                                    Area = (double)readerCountry["area"],
                                    Gini = (double)readerCountry["gini"],
                                    NativeName = (string)readerCountry["native_name"],
                                    NumericCode = (string)readerCountry["numeric_code"],
                                    Flag = (string)readerCountry["flag"],
                                    Cioc = (string)readerCountry["cioc"]
                                };

                                country.Name.Replace("´", "'");
                                country.Capital.Replace("´", "'");
                                country.NativeName.Replace("´", "'");

                                using (SQLiteCommand command2 = new SQLiteCommand(connection))
                                {
                                    //Get data from the table top_level_domain
                                    command2.CommandText = $"select top_level_domain from top_level_domain where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerTopLevelDomain = command2.ExecuteReader())
                                    {
                                        List<string> TopLevelDomain = new List<string>();

                                        while (readerTopLevelDomain.Read())
                                        {
                                            string topLevelDomain = (string)readerTopLevelDomain["top_level_domain"];

                                            TopLevelDomain.Add(topLevelDomain);
                                        }

                                        country.TopLevelDomain = TopLevelDomain;
                                    }


                                    //Get data from the table calling_code
                                    command2.CommandText = $"select calling_codes from calling_code where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerCallingCodes = command2.ExecuteReader())
                                    {
                                        List<string> CallingCodes = new List<string>();

                                        while (readerCallingCodes.Read())
                                        {
                                            string callingCode = (string)readerCallingCodes["calling_codes"];

                                            CallingCodes.Add(callingCode);
                                        }

                                        country.CallingCodes = CallingCodes;
                                    }

                                    //Get data from the table alt_spelling
                                    command2.CommandText = $"select alt_spellings from alt_spelling where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerAltSpellings = command2.ExecuteReader())
                                    {
                                        List<string> AltSpellings = new List<string>();

                                        while (readerAltSpellings.Read())
                                        {
                                            string altSpelling = (string)readerAltSpellings["alt_spellings"];

                                            altSpelling.Replace("´", "'");

                                            AltSpellings.Add(altSpelling);
                                        }

                                        country.AltSpellings = AltSpellings;
                                    }

                                    //Get data from the table lat_lng
                                    command2.CommandText = $"select lat, lng from lat_lng where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerLatLng = command2.ExecuteReader())
                                    {
                                        List<double> LatLng = new List<double>();

                                        while (readerLatLng.Read())
                                        {
                                            double lat = (double)readerLatLng["lat"];
                                            double lng = (double)readerLatLng["lng"];

                                            LatLng.Add(lat);
                                            LatLng.Add(lng);
                                        }

                                        country.Latlng = LatLng;
                                    }

                                    //Get data from the table country_timezone
                                    command2.CommandText = $"select timezones from country_timezone where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerTimezone = command2.ExecuteReader())
                                    {
                                        List<string> Timezones = new List<string>();

                                        while (readerTimezone.Read())
                                        {
                                            string timezone = (string)readerTimezone["timezones"];

                                            Timezones.Add(timezone);
                                        }

                                        country.Timezones = Timezones;
                                    }

                                    //Get data from the table country_border
                                    command2.CommandText = $"select borders from country_border where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerBorder = command2.ExecuteReader())
                                    {
                                        List<string> Borders = new List<string>();

                                        while (readerBorder.Read())
                                        {
                                            string border = (string)readerBorder["borders"];

                                            Borders.Add(border);
                                        }

                                        country.Borders = Borders;
                                    }

                                    //Get data from the table translation
                                    command2.CommandText = $"select de, es, fr, ja, it, br, pt, nl, hr, fa from translation where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerTranslation = command2.ExecuteReader())
                                    {
                                        Translation translation = new Translation();

                                        while (readerTranslation.Read())
                                        {
                                            translation.De = (string)readerTranslation["de"];
                                            translation.Es = (string)readerTranslation["es"];
                                            translation.Fr = (string)readerTranslation["fr"];
                                            translation.Ja = (string)readerTranslation["ja"];
                                            translation.It = (string)readerTranslation["it"];
                                            translation.Br = (string)readerTranslation["br"];
                                            translation.Pt = (string)readerTranslation["pt"];
                                            translation.Nl = (string)readerTranslation["nl"];
                                            translation.Hr = (string)readerTranslation["hr"];
                                            translation.Fa = (string)readerTranslation["fa"];
                                        }

                                        translation.Fr.Replace("´", "'");
                                        translation.It.Replace("´", "'");

                                        if (translation.It == string.Empty)
                                        {
                                            translation.It = null;
                                        }

                                        if (translation.Fr == string.Empty)
                                        {
                                            translation.Fr = null;
                                        }

                                        country.Translations = translation;
                                    }

                                    //Get data from the tables country_currency and currency
                                    command2.CommandText = $"select code_currency, name, symbol from currency inner join country_currency on code=code_currency where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerCurrency = command2.ExecuteReader())
                                    {
                                        List<Currency> Currencies = new List<Currency>();

                                        while (readerCurrency.Read())
                                        {
                                            Currency currency = new Currency
                                            {
                                                Code = (string)readerCurrency["code_currency"],
                                                Name = (string)readerCurrency["name"],
                                                Symbol = (string)readerCurrency["symbol"]
                                            };

                                            currency.Name.Replace("´", "'");

                                            Currencies.Add(currency);
                                        }

                                        country.Currencies = Currencies;
                                    }

                                    //Get data from the tables language and country_language
                                    command2.CommandText = $"select iso639_1_language, iso639_2, name, native_name from language inner join country_language on iso639_1=iso639_1_language where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerLanguage = command2.ExecuteReader())
                                    {
                                        List<Language> Languages = new List<Language>();

                                        while (readerLanguage.Read())
                                        {
                                            Language language = new Language
                                            {
                                                Iso639_1 = (string)readerLanguage["iso639_1_language"],
                                                Iso639_2 = (string)readerLanguage["iso639_2"],
                                                Name = (string)readerLanguage["name"],
                                                NativeName = (string)readerLanguage["native_name"]
                                            };

                                            language.NativeName.Replace("´", "'");

                                            Languages.Add(language);
                                        }

                                        country.Languages = Languages;
                                    }

                                    //Get data from the tables regional_bloc and country_regional_bloc
                                    command2.CommandText = $"select acronym_regional_bloc, name from regional_bloc inner join country_regional_bloc on acronym=acronym_regional_bloc where country_alpha3code='{country.Alpha3Code}'";

                                    using (SQLiteDataReader readerRegionalBloc = command2.ExecuteReader())
                                    {
                                        List<RegionalBloc> RegionalBlocs = new List<RegionalBloc>();

                                        while (readerRegionalBloc.Read())
                                        {
                                            RegionalBloc regionalBloc = new RegionalBloc
                                            {
                                                Acronym = (string)readerRegionalBloc["acronym_regional_bloc"],
                                                Name = (string)readerRegionalBloc["name"]
                                            };

                                            using (SQLiteCommand command3 = new SQLiteCommand(connection))
                                            {
                                                //Get data from the table other_acronym
                                                command3.CommandText = $"select other_acronyms from other_acronym inner join regional_bloc on acronym=acronym_regional_bloc where acronym_regional_bloc='{regionalBloc.Acronym}'";

                                                using (SQLiteDataReader readerOtherAcronym = command3.ExecuteReader())
                                                {
                                                    List<string> OtherAcronyms = new List<string>();

                                                    while (readerOtherAcronym.Read())
                                                    {
                                                        string otherAcronym = (string)readerOtherAcronym["other_acronyms"];

                                                        OtherAcronyms.Add(otherAcronym);
                                                    }

                                                    regionalBloc.OtherAcronyms = OtherAcronyms;
                                                }

                                                //Get data from the table other_name 
                                                command3.CommandText = $"select other_names from other_name inner join regional_bloc on acronym=acronym_regional_bloc where acronym_regional_bloc='{regionalBloc.Acronym}'";

                                                using (SQLiteDataReader readerOtherName = command3.ExecuteReader())
                                                {
                                                    List<string> OtherNames = new List<string>();

                                                    while (readerOtherName.Read())
                                                    {
                                                        string otherName = (string)readerOtherName["other_names"];

                                                        OtherNames.Add(otherName);
                                                    }

                                                    regionalBloc.OtherNames = OtherNames;
                                                }
                                            }

                                            RegionalBlocs.Add(regionalBloc);
                                        }

                                        country.RegionalBlocs = RegionalBlocs;
                                    }
                                }

                                Countries.Add(country);
                                report.PercentageComplete = (Countries.Count * 100) / 250;
                                progress.Report(report);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    DialogService.ShowMessageBox("Error", e.Message);
                }
            });

            return Countries;
        }
    }
}
