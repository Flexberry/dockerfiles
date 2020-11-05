[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading;
    using ICSSoft.STORMNET.Business;
    using Npgsql;
    using Oracle.ManagedDataAccess.Client;
    using Xunit;
    using Xunit.Abstractions;

#if NETFRAMEWORK
    /// <summary>
    /// Base class for integration tests.
    /// </summary>
    public abstract class BaseIntegratedTest : IDisposable
    {
#endif
#if NETCOREAPP
    using Microsoft.AspNetCore.Mvc.Testing;
    using ODataServiceSample.AspNetCore;
    using ICSSoft.Services;
    using Unity;

    /// <summary>
    /// Base class for integration tests.
    /// </summary>
    public abstract class BaseIntegratedTest : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        protected readonly WebApplicationFactory<Startup> _factory;
#endif
        protected ITestOutputHelper _output;

        private const string PoolingFalseConst = "Pooling=false;";

        private static string connectionStringOracle;

        private static string connectionStringPostgres;

        private static string connectionStringMssql;

        /// <summary>
        /// The temporary database name prefix.
        /// </summary>
        private readonly string _tempDbNamePrefix;

        private string _databaseName;

        private string _tmpUserNameOracle;

        /// <summary>
        /// The data services for temp databases (for <see cref="DataServices"/>).
        /// </summary>
        private readonly List<IDataService> _dataServices = new List<IDataService>();

        private bool _useGisDataService;

        /// <summary>
        /// Flag: Indicates whether "Dispose" has already been called.
        /// </summary>
        private bool _disposed;

        protected virtual string MssqlScript
        {
            get
            {
                return Resources.MssqlScript;
            }
        }

        protected virtual string PostgresScript
        {
            get
            {
                return Resources.PostgresScript;
            }
        }

        protected virtual string OracleScript
        {
            get
            {
                return Resources.OracleScript;
            }
        }

        /// <summary>
        /// Data services for temp databases.
        /// </summary>
        protected IEnumerable<IDataService> DataServices
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(null);

                return _dataServices;
            }
        }

        static BaseIntegratedTest()
        {
            // ADO.NET doesn't close the connection with pooling. We have to disable it explicitly.
            // http://stackoverflow.com/questions/9033356/connection-still-idle-after-close
            connectionStringPostgres = $"{PoolingFalseConst}{ConfigurationManager.ConnectionStrings["ConnectionStringPostgres"]}";
            connectionStringMssql = $"{PoolingFalseConst}{ConfigurationManager.ConnectionStrings["ConnectionStringMssql"]}";
            connectionStringOracle = $"{PoolingFalseConst}{ConfigurationManager.ConnectionStrings["ConnectionStringOracle"]}";
        }

        /// <summary>
        /// Deletes the temporary databases and perform other cleaning.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

#if NETFRAMEWORK
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseIntegratedTest" /> class.
        /// </summary>
        /// <param name="tempDbNamePrefix">Prefix for temp database name.</param>
        /// <param name="useGisDataService">Use DataService with Gis support.</param>
        protected BaseIntegratedTest(string tempDbNamePrefix, bool useGisDataService = false)
        {
#endif
#if NETCOREAPP
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseIntegratedTest" /> class.
        /// </summary>
        /// <param name="factory">Web application factory.</param>
        /// <param name="output">Unit tests debug output.</param>
        /// <param name="tempDbNamePrefix">Prefix for temp database name.</param>
        /// <param name="useGisDataService">Use DataService with Gis support.</param>
        protected BaseIntegratedTest(CustomWebApplicationFactory<Startup> factory, ITestOutputHelper output, string tempDbNamePrefix, bool useGisDataService = false)
        {
            _factory = factory;
            _output = output;

            if (output != null)
            {
                IUnityContainer container = UnityFactory.GetContainer();
                container.RegisterInstance(_output);
            }
#endif
            _useGisDataService = useGisDataService;
            if (!(tempDbNamePrefix != null))
                throw new ArgumentNullException();
            if (!(tempDbNamePrefix != string.Empty))
                throw new ArgumentException();
            if (!tempDbNamePrefix.All(char.IsLetterOrDigit))
                throw new ArgumentException();
            _tempDbNamePrefix = tempDbNamePrefix;
            _databaseName = _tempDbNamePrefix + "_" + DateTime.Now.ToString("yyyyMMddHHmmssff") + "_" + Guid.NewGuid().ToString("N");
            bool watchdogEmptyTest = false;

            if (!string.IsNullOrWhiteSpace(PostgresScript) && connectionStringPostgres != PoolingFalseConst)
            {
                if (!(tempDbNamePrefix.Length <= 12)) // Max length is 63 (-18 -32).
                    throw new ArgumentException();

                if (!char.IsLetter(tempDbNamePrefix[0])) // Database names must have an alphabetic first character.
                    throw new ArgumentException();

                watchdogEmptyTest = true;

                using (var conn = new NpgsqlConnection(connectionStringPostgres))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(string.Format("CREATE DATABASE \"{0}\" ENCODING = 'UTF8' CONNECTION LIMIT = -1;", _databaseName), conn))
                        cmd.ExecuteNonQuery();
                }

                using (var conn = new NpgsqlConnection($"{connectionStringPostgres};Database={_databaseName}"))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("CREATE EXTENSION postgis;", conn) { CommandTimeout = 60 })
                        cmd.ExecuteNonQuery();
                    using (var cmd = new NpgsqlCommand(PostgresScript, conn))
                        cmd.ExecuteNonQuery();
                    _dataServices.Add(CreatePostgresDataService($"{connectionStringPostgres};Database={_databaseName}"));
                }
            }

            if (!string.IsNullOrWhiteSpace(MssqlScript) && connectionStringMssql != PoolingFalseConst)
            {
                if (!(tempDbNamePrefix.Length <= 64))// Max is 128.
                    throw new ArgumentException();

                watchdogEmptyTest = true;

                using (var connection = new SqlConnection(connectionStringMssql))
                {
                    connection.Open();
                    using (var command = new SqlCommand($"CREATE DATABASE {_databaseName} COLLATE Cyrillic_General_CI_AS", connection))
                        command.ExecuteNonQuery();
                }

                using (var connection = new SqlConnection($"{connectionStringMssql};Database={_databaseName}"))
                {
                    connection.Open();
                    using (var command = new SqlCommand(MssqlScript, connection))
                    {
                        command.CommandTimeout = 180;
                        command.ExecuteNonQuery();
                    }

                    _dataServices.Add(CreateMssqlDataService($"{connectionStringMssql};Database={_databaseName}"));
                }
            }

            if (!string.IsNullOrWhiteSpace(OracleScript) && connectionStringOracle != PoolingFalseConst)
            {
                if (!(tempDbNamePrefix.Length <= 8)) // Max length is 30 (-18 -4).
                    throw new ArgumentException();

                watchdogEmptyTest = true;

                using (var connection = new OracleConnection(connectionStringOracle))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        // "CREATE USER" privileges required.
                        var doWhile = true;
                        while (doWhile)
                        {
                            _tmpUserNameOracle = tempDbNamePrefix + "_" + DateTime.Now.ToString("yyyyMMddHHmmssff") + "_" + new Random().Next(9999);
                            command.CommandText = $"CREATE USER {_tmpUserNameOracle} IDENTIFIED BY {_tmpUserNameOracle} DEFAULT TABLESPACE users  quota unlimited on users  TEMPORARY TABLESPACE temp";
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (OracleException ex)
                            {
                                Thread.Sleep(1000);
                                if (ex.Message.Contains("conflicts with another user or role name "))
                                    continue;
                                throw;
                            }

                            doWhile = false;
                        }

                        // "CREATE SESSION WITH ADMIN OPTION" privileges required.
                        command.CommandText = $"GRANT CREATE SESSION TO {_tmpUserNameOracle}";
                        command.ExecuteNonQuery();
                        command.CommandText = $"GRANT CREATE TABLE TO {_tmpUserNameOracle}";
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new OracleConnection($"{ConnectionStringOracleDataSource};User Id={_tmpUserNameOracle};Password={_tmpUserNameOracle};"))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        foreach (var cmdText in OracleScript.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            command.CommandText = cmdText.Trim();
                            if (!string.IsNullOrWhiteSpace(command.CommandText))
                                command.ExecuteNonQuery();
                        }

                        _dataServices.Add(CreateOracleDataService($"{ConnectionStringOracleDataSource};User Id={_tmpUserNameOracle};Password={_tmpUserNameOracle};"));
                    }
                }
            }

            Assert.True(watchdogEmptyTest);
        }

        /// <summary>
        /// Creates the <see cref="MSSQLDataService"/> instance for temp database.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="MSSQLDataService"/> instance.</returns>
        protected virtual MSSQLDataService CreateMssqlDataService(string connectionString)
        {
            if (_useGisDataService)
                return new GisMSSQLDataService { CustomizationString = connectionString };
            return new MSSQLDataService { CustomizationString = connectionString };
        }

        /// <summary>
        /// Creates the <see cref="PostgresDataService"/> instance for temp database.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="PostgresDataService"/> instance.</returns>
        protected virtual PostgresDataService CreatePostgresDataService(string connectionString)
        {
            if (_useGisDataService)
                return new GisPostgresDataService { CustomizationString = connectionString };
            return new PostgresDataService { CustomizationString = connectionString };
        }

        /// <summary>
        /// Creates the <see cref="OracleDataService"/> instance for temp database.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="OracleDataService"/> instance.</returns>
        protected virtual OracleDataService CreateOracleDataService(string connectionString)
        {
            return new OracleDataService { CustomizationString = connectionString };
        }

        /// <summary>
        /// Deletes the temporary databases and perform other cleaning.
        /// </summary>
        /// <param name="disposing">Flag: indicates whether method is calling from "Dispose()" or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try
                {
                    foreach (var ds in _dataServices)
                    {
                        if (ds is PostgresDataService || ds.GetType().IsSubclassOf(typeof(PostgresDataService)))
                        {
                            using (var conn = new NpgsqlConnection(connectionStringPostgres))
                            {
                                conn.Open();
                                using (var command = new NpgsqlCommand($"DROP DATABASE \"{_databaseName}\";", conn))
                                    command.ExecuteNonQuery();
                            }
                        }

                        if (ds is MSSQLDataService || ds.GetType().IsSubclassOf(typeof(MSSQLDataService)))
                        {
                            using (var connection = new SqlConnection(connectionStringMssql))
                            {
                                connection.Open();
                                using (var command = new SqlCommand($"DROP DATABASE {_databaseName}", connection))
                                    command.ExecuteNonQuery();
                            }
                        }

                        if (ds is OracleDataService || ds.GetType().IsSubclassOf(typeof(OracleDataService)))
                        {
                            using (var connection = new OracleConnection(connectionStringOracle))
                            {
                                connection.Open();
                                using (var command = connection.CreateCommand())
                                {
                                    command.CommandText = $"DROP USER {_tmpUserNameOracle} CASCADE";
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            _disposed = true;
        }

        private static string ConnectionStringOracleDataSource
        {
            get
            {
                var dataSource = connectionStringOracle.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(t => t.Trim().ToLower().IndexOf("data") == 0 && t.ToLower().IndexOf("source") != -1);

                // ADO.NET doesn't close the connection with pooling. We have to disable it explicitly.
                // http://stackoverflow.com/questions/9033356/connection-still-idle-after-close
                return $"{PoolingFalseConst}{dataSource};";
            }
        }
    }
}
