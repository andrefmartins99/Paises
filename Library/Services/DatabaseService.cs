using Library.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Library.Services
{
    public static class DatabaseService
    {
        /// <summary>
        /// Save the data retrieved from the api in the database
        /// </summary>
        /// <param name="Countries">List of countries</param>
        public static async Task SaveData(List<Country> Countries)
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

                    await InsertData(command, Countries, Languages, Currencies, RegionalBlocs);
                }
            }
        }

        /// <summary>
        /// Get distinct languages, currencies and regional blocs
        /// </summary>
        /// <param name="Countries">list of countries</param>
        /// <param name="Languages">list of languages (empty)</param>
        /// <param name="Currencies">list of currencies (empty)</param>
        /// <param name="RegionalBlocs">list of regional blocs (empty)</param>
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
        /// <param name="command">sqlite command</param>
        private static async Task CreateDatabase(SQLiteCommand command)
        {
            await Task.Run(() =>
            {
                try
                {
                    //Table country
                    command.CommandText = "create table if not exists country(alpha3code char(3) primary key, name varchar(100), alpha2code char(2), capital varchar(50), region varchar(10), subregion varchar(30), population int, demonym varchar(50), area real, gini real, native_name nvarchar(100), numeric_code varchar(3), flag blob, cioc varchar(3))";
                    command.ExecuteNonQuery();

                    //Table top_level_domain
                    command.CommandText = "create table if not exists top_level_domain(id integer primary key autoincrement, top_level_domain varchar(3), country_alpha3code char(3) references country(alpha3code))";
                    command.ExecuteNonQuery();

                    //Table calling_code
                    command.CommandText = "create table if not exists calling_code(id integer primary key autoincrement, calling_codes varchar(5), country_alpha3code char(3) references country(alpha3code))";
                    command.ExecuteNonQuery();

                    //Table alt_spelling
                    command.CommandText = "create table if not exists alt_spelling(id integer primary key autoincrement, alt_spellings nvarchar(100), country_alpha3code char(3) references country(alpha3code))";
                    command.ExecuteNonQuery();

                    //Table lat_lng
                    command.CommandText = "create table if not exists lat_lng(country_alpha3code char(3) primary key references country(alpha3code), lat varchar(20), lng varchar(20))";
                    command.ExecuteNonQuery();

                    //Table translation
                    command.CommandText = "create table if not exists translation(country_alpha3code char(3) primary key references country(alpha3code), de nvarchar(100), es nvarchar(100), fr nvarchar(100), ja nvarchar(100), it nvarchar(100), br nvarchar(100), pt nvarchar(100), nl nvarchar(100), hr nvarchar(100), fa nvarchar(100))";
                    command.ExecuteNonQuery();

                    //Table country_border
                    command.CommandText = "create table if not exists country_border(country_alpha3code char(3) references country(alpha3code), borders varchar(3), primary key(country_alpha3code, borders))";
                    command.ExecuteNonQuery();

                    //Table country_timezone
                    command.CommandText = "create table if not exists country_timezone(country_alpha3code char(3) references country(alpha3code), timezones varchar(9), primary key(country_alpha3code, timezones))";
                    command.ExecuteNonQuery();

                    //Table language
                    command.CommandText = "create table if not exists language(iso639_1 varchar(5) primary key, iso639_2 varchar(5), name varchar(50), native_name nvarchar(50))";
                    command.ExecuteNonQuery();

                    //Table country_language
                    command.CommandText = "create table if not exists country_language(country_alpha3code char(3) references country(alpha3code), iso639_1_language varchar(5) references language(iso639_1), primary key(country_alpha3code, iso639_1_language))";
                    command.ExecuteNonQuery();

                    //Table currency
                    command.CommandText = "create table if not exists currency(code varchar(5) primary key, name varchar(50), symbol nvarchar(5))";
                    command.ExecuteNonQuery();

                    //Table country_currency
                    command.CommandText = "create table if not exists country_currency(country_alpha3code char(3) references country(alpha3code), code_currency varchar(5) references currency(code), primary key(country_alpha3code, code_currency))";
                    command.ExecuteNonQuery();

                    //Table regional_bloc
                    command.CommandText = "create table if not exists regional_bloc(acronym varchar(10) primary key, name varchar(50))";
                    command.ExecuteNonQuery();

                    //Table country_regional_bloc
                    command.CommandText = "create table if not exists country_regional_bloc(country_alpha3code char(3) references country(alpha3code), acronym_regional_bloc varchar(10) references regional_bloc(acronym), primary key(country_alpha3code, acronym_regional_bloc))";
                    command.ExecuteNonQuery();

                    //Table other_acronym
                    command.CommandText = "create table if not exists other_acronym(id integer primary key autoincrement, other_acronyms varchar(10), acronym_regional_bloc varchar(10) references regional_bloc(acronym))";
                    command.ExecuteNonQuery();

                    //Table other_name
                    command.CommandText = "create table if not exists other_name(id integer primary key autoincrement, other_names nvarchar(100), acronym_regional_bloc varchar(10) references regional_bloc(acronym))";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    DialogService.ShowMessageBox("Erro", e.Message);
                }
            });
        }

        /// <summary>
        /// Delete data from the database
        /// </summary>
        /// <param name="command">sqlite command</param>
        /// <returns></returns>
        private static async Task DeleteData(SQLiteCommand command)
        {
            await Task.Run(() =>
            {
                try
                {
                    //Delete data from table country_regional_bloc
                    command.CommandText = "delete from country_regional_bloc";
                    command.ExecuteNonQuery();

                    //Delete data from table country_currency
                    command.CommandText = "delete from country_currency";
                    command.ExecuteNonQuery();

                    //Delete data from table country_language
                    command.CommandText = "delete from country_language";
                    command.ExecuteNonQuery();

                    //Delete data from table country_timezone
                    command.CommandText = "delete from country_timezone";
                    command.ExecuteNonQuery();

                    //Delete data from table country_border
                    command.CommandText = "delete from country_border";
                    command.ExecuteNonQuery();

                    //Delete data from table translation
                    command.CommandText = "delete from translation";
                    command.ExecuteNonQuery();

                    //Delete data from table lat_lng
                    command.CommandText = "delete from lat_lng";
                    command.ExecuteNonQuery();

                    //Delete data from table alt_spelling
                    command.CommandText = "delete from alt_spelling";
                    command.ExecuteNonQuery();

                    //Delete data from table calling_code
                    command.CommandText = "delete from calling_code";
                    command.ExecuteNonQuery();

                    //Delete data from table top_level_domain
                    command.CommandText = "delete from top_level_domain";
                    command.ExecuteNonQuery();

                    //Delete data from table country
                    command.CommandText = "delete from country";
                    command.ExecuteNonQuery();

                    //Delete data from table other_name
                    command.CommandText = "delete from other_name";
                    command.ExecuteNonQuery();

                    //Delete data from table other_acronym
                    command.CommandText = "delete from other_acronym";
                    command.ExecuteNonQuery();

                    //Delete data from table regional_bloc
                    command.CommandText = "delete from regional_bloc";
                    command.ExecuteNonQuery();

                    //Delete data from table currency
                    command.CommandText = "delete from currency";
                    command.ExecuteNonQuery();

                    //Delete data from table language
                    command.CommandText = "delete from language";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    DialogService.ShowMessageBox("Erro", e.Message);
                }
            });
        }

        /// <summary>
        /// Insert the data into the database
        /// </summary>
        /// <param name="command">sqlite command</param>
        /// <param name="Countries">list of countries</param>
        /// <param name="Languages">list of distinct languages</param>
        /// <param name="Currencies">list of distinct currencies</param>
        /// <param name="RegionalBlocs">list of distinct regional blocs</param>
        private static async Task InsertData(SQLiteCommand command, List<Country> Countries, List<Language> Languages, List<Currency> Currencies, List<RegionalBloc> RegionalBlocs)
        {
            await Task.Run(() =>
            {
                try
                {
                    //Fill table language with data
                    foreach (var language in Languages)
                    {
                        command.CommandText = $"insert into language values('{language.Iso639_1}','{language.Iso639_2}','{language.Name}','{language.NativeName.Replace("'", "´")}')";
                        command.ExecuteNonQuery();
                    }

                    //Fill table currency with data
                    foreach (var currency in Currencies)
                    {
                        command.CommandText = $"insert into currency values('{currency.Code}','{currency.Name.Replace("'", "´")}','{currency.Symbol}')";
                        command.ExecuteNonQuery();
                    }

                    foreach (var regionalBloc in RegionalBlocs)
                    {
                        //Fill table regional_bloc with data
                        command.CommandText = $"insert into regional_bloc values('{regionalBloc.Acronym}','{regionalBloc.Name}')";
                        command.ExecuteNonQuery();

                        //Fill table other_acronym with data
                        foreach (var otherAcronym in regionalBloc.OtherAcronyms)
                        {
                            command.CommandText = $"insert into other_acronym(other_acronyms,acronym_regional_bloc) values('{otherAcronym}','{regionalBloc.Acronym}')";
                            command.ExecuteNonQuery();
                        }

                        //Fill table other_name with data
                        foreach (var otherName in regionalBloc.OtherNames)
                        {
                            command.CommandText = $"insert into other_name(other_names,acronym_regional_bloc) values('{otherName}','{regionalBloc.Acronym}')";
                            command.ExecuteNonQuery();
                        }
                    }

                    foreach (var country in Countries)
                    {
                        //Fill table country with data
                        command.CommandText = $"insert into country values('{country.Alpha3Code}','{country.Name.Replace("'", "´")}','{country.Alpha2Code}','{country.Capital.Replace("'", "´")}','{country.Region}','{country.Subregion}',{country.Population},'{country.Demonym}','{country.Area}','{country.Gini}','{country.NativeName.Replace("'", "´")}','{country.NumericCode}','{country.Flag}','{country.Cioc}')";
                        command.ExecuteNonQuery();

                        //Fill table top_level_domain with data
                        foreach (var topLevelDomain in country.TopLevelDomain)
                        {
                            command.CommandText = $"insert into top_level_domain(top_level_domain,country_alpha3code) values('{topLevelDomain}','{country.Alpha3Code}')";
                            command.ExecuteNonQuery();
                        }

                        //Fill table calling_code with data
                        foreach (var callingCode in country.CallingCodes)
                        {
                            command.CommandText = $"insert into calling_code(calling_codes,country_alpha3code) values('{callingCode}','{country.Alpha3Code}')";
                            command.ExecuteNonQuery();
                        }

                        //Fill table alt_spelling with data
                        foreach (var altSpelling in country.AltSpellings)
                        {
                            command.CommandText = $"insert into alt_spelling(alt_spellings,country_alpha3code) values('{altSpelling.Replace("'", "´")}','{country.Alpha3Code}')";
                            command.ExecuteNonQuery();
                        }

                        //Fill table lat_lng with data
                        if (country.Latlng.Count > 0)
                        {
                            command.CommandText = $"insert into lat_lng values('{country.Alpha3Code}','{country.Latlng[0]}','{country.Latlng[1]}')";
                            command.ExecuteNonQuery();
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
                        command.ExecuteNonQuery();

                        //Fill table country_border with data
                        foreach (var border in country.Borders)
                        {
                            command.CommandText = $"insert into country_border values('{country.Alpha3Code}','{border}')";
                            command.ExecuteNonQuery();
                        }

                        //Fill table country_timezone with data
                        foreach (var timeZone in country.Timezones)
                        {
                            command.CommandText = $"insert into country_timezone values('{country.Alpha3Code}','{timeZone}')";
                            command.ExecuteNonQuery();
                        }

                        //Fill table country_language with data
                        foreach (var language in country.Languages)
                        {
                            command.CommandText = $"insert into country_language values('{country.Alpha3Code}','{language.Iso639_1}')";
                            command.ExecuteNonQuery();
                        }

                        //Fill table country_currency with data
                        foreach (var currency in country.Currencies)
                        {
                            command.CommandText = $"insert into country_currency values('{country.Alpha3Code}','{currency.Code}')";
                            command.ExecuteNonQuery();
                        }

                        //Fill table country_regional_bloc with data
                        foreach (var regionalBloc in country.RegionalBlocs)
                        {
                            command.CommandText = $"insert into country_regional_bloc values('{country.Alpha3Code}','{regionalBloc.Acronym}')";
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    DialogService.ShowMessageBox("Erro", e.Message);
                }
            });
        }
    }
}
