using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .AddUserSecrets("ca58807f-0ed1-4363-8db8-956fdb027ad5")
                 .Build();


var connString = config.GetConnectionString("DefaultConnection");
var duediligences = new List<DueDiligence>();
var processamentos = new List<Processamento>();
var qtdProcessamentos = 0;

//******************* query simples
Console.WriteLine("ÚLTIMAS 10 DUE DILIGENCES");
using (var connection = new SqlConnection(connString))
{
    //connection.Open();
    var sql = "select top 10 * from duediligences order by 1 desc";
    duediligences = connection.Query<DueDiligence>(sql).AsList();
}

foreach (var duediligence in duediligences)
{
    Console.WriteLine($"{duediligence.Nome}, {duediligence.DataCriacao}");
}

//******************* query com parâmetros
Console.WriteLine("\nDUE DILIGENCES CRIADAS NOS ÚLTIMOS 10 DIAS");
using (var connection = new SqlConnection(connString))
{
    //connection.Open();
    var dictionary = new Dictionary<string, object>
    {
        { "@DataCriacao", DateTime.Now.AddDays(-10) }
    };
    var parameters = new DynamicParameters(dictionary);
    var sql = "select * from duediligences where DataCriacao >= @DataCriacao order by 1 desc";
    duediligences = connection.Query<DueDiligence>(sql, parameters).AsList();
}

foreach (var duediligence in duediligences)
{
    Console.WriteLine($"{duediligence.Nome}, {duediligence.DataCriacao}");
}

//******************* query com inner join
Console.WriteLine("\nQUANTIDADE DE PROCESSAMENTOS");
using (var connection = new SqlConnection(connString))
{
    //connection.Open();
    var sql = "select * from processamentos p inner join duediligences d on d.id = p.idduediligence";
    //processamentos = connection.Query<Processamento>(sql).AsList();
    processamentos = (await connection.QueryAsync<Processamento, DueDiligence, Processamento>(sql, (processamento, dd) => {
        processamento.DueDiligence = dd;
        return processamento;
    },
    splitOn: "IdDueDiligence")).AsList();
}

Console.WriteLine(processamentos.Count);

//******************* query com execute scalar
Console.WriteLine("\nQUANTIDADE DE PROCESSAMENTOS (COM EXECUTESCALAR)");
using (var connection = new SqlConnection(connString))
{
    //connection.Open();
    var sql = "select count(1) from processamentos p inner join duediligences d on d.id = p.idduediligence";
    qtdProcessamentos = await connection.ExecuteScalarAsync<int>(sql);
}

Console.WriteLine(qtdProcessamentos);

class Processamento
{
    public int Id { get; set; }
    public int IdDueDiligence { get; set; }
    public DateTime DataCriacao { get; set; }
    public DueDiligence DueDiligence { get; set; }
}

class DueDiligence
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public DateTime DataCriacao { get; set; }
    public int IdUsuario { get; set; }
}