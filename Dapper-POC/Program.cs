using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .AddUserSecrets("ca58807f-0ed1-4363-8db8-956fdb027ad5")
                 .Build();


var connString = config.GetConnectionString("DefaultConnection");
var processamentos = new List<Processamento>();

using (var connection = new SqlConnection(connString))
{
    connection.Open();
    var sql = "select * from processamentos p inner join duediligences d on d.id = p.idduediligence";
    //processamentos = connection.Query<Processamento>(sql).AsList();
    processamentos = (await connection.QueryAsync<Processamento, DueDiligence, Processamento>(sql, (processamento, dd) => {
        processamento.DueDiligence = dd;
        return processamento;
    },
    splitOn: "IdDueDiligence")).AsList();
}

Console.WriteLine(processamentos.Count);

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