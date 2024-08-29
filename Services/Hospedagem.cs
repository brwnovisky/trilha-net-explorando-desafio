using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json;
using DesafioProjetoHospedagem.CLI;
using DesafioProjetoHospedagem.Models;

namespace DesafioProjetoHospedagem.Services;

public class Hospedagem
{
    private Dictionary<string, (Pessoa, Reserva)> Hospedes;
    private Dictionary<string, (Suite, Reserva)> Suites;
    private Queue<Reserva> Reservas;
    private string rotaMenu = "/Home";
    private string PastaBancoDeDados = "Database/";
    
    public void IniciarServico()
    {
        CarregarBancoDeDados();
        Sessao();
        AtualizarBancoDeDados();
        
        Console.CursorVisible = true;
        Console.Clear();
    }
    
    private void Sessao()
    {
        while (Ativa()) {}
    }
    
    private bool Ativa()
    {
        Menu menu = new Menu();
        rotaMenu = menu.Interface(rotaMenu);
        var (hospede, suite, reserva) = menu; 
        
        string resposta = "Nenhuma Suíte Disponível!";
        
        switch (rotaMenu)
        {
            case "/Home/Hospedes/Cadastrar_Hospede":
                resposta = AdicionarHospede(hospede);
                break;
            
            case "/Home/Hospedes/Buscar_Hospede":
            case "/Home/Hospedes/Listar_Hospedes":
                resposta = AcessarHospedes(hospede);
                break;
            
            case "/Home/Reservas/Cadastrar_Reserva":
                var listaSuites = AcessarListaSuites();
                if (listaSuites.Count < 3) break;
                menu.AdicionarRota(rotaMenu, listaSuites);
                var rotaSuite = menu.Interface(rotaMenu).Replace(rotaMenu + '/', "");
                var suiteSelecionada = Suites[rotaSuite];
                menu.FormularioDeReservas(rotaMenu, suiteSelecionada.Item1);
                var (_, _, novaReserva) = menu;
                resposta = AdicionarReserva(novaReserva);
                rotaMenu = "/Home/Reservas";
                break;
            
            case "/Home/Reservas/Buscar_Reserva":
            case "/Home/Reservas/Listar_Reservas":
                resposta = AcessarReservas(reserva);
                break;
            
            case "/Home/Suites/Cadastrar_Suite":
                resposta = AdicionarSuite(suite);
                break;
            
            case "/Home/Suites/Buscar_Suite":
            case "/Home/Suites/Listar_Suites":
                resposta = AcessarSuites(suite);
                break;
            
           case null: return false; 
            
            default:
            { 
                menu.Interface(rotaMenu);
                break;
            }
        }
        
        menu.Resposta(resposta, rotaMenu);
        return true;
    }

    private string AdicionarHospede(Pessoa novoHospede, Reserva reserva = null)
    {
        bool sucesso = Hospedes.TryAdd(novoHospede.NomeCompleto, (novoHospede, reserva));
                
        string resposta = sucesso ? "Hóspede cadastrado com sucesso!" : "Este hóspede já foi cadastrado!";
        
        rotaMenu = "/Home/Hospedes";

        return resposta;
    }
    
    private string AcessarHospedes(Pessoa novoHospede = null)
    {
        string resposta = "Hóspede ainda não cadastrado!";
        
        rotaMenu = "/Home/Hospedes";

        if (novoHospede != null)
        {
            bool sucesso = Hospedes.TryGetValue(novoHospede.NomeCompleto, out var registro);
            resposta = sucesso ? FormatarHospede(registro.Item1, registro.Item2) : resposta;
            return resposta;
        } 
        
        resposta = Hospedes.Count > 0 ? "" : "Nenhum hóspede cadastrado!";
        
        foreach (var (nomeCompleto, pessoa) in Hospedes)
        {
            resposta += FormatarHospede(pessoa.Item1, pessoa.Item2);
        }
        
        return resposta;
    }

    private string FormatarHospede(Pessoa hospede, Reserva reserva)
    {
        string formatado = $"          Nome: {hospede.Nome}" +
                           $"\n     Sobrenome: {hospede.Sobrenome}" +
                           $"\n Nome Completo: {hospede.NomeCompleto}" +
                           $"\nPossui Reserva: {(reserva != null ? "Sim": "Não")}\n\n";

        return formatado;
    }
    
    private string AdicionarSuite(Suite novaSuite)
    {
        bool sucesso = Suites.TryAdd(novaSuite.TipoSuite, (novaSuite, null));
                
        string resposta = sucesso ? "Suíte cadastrado com sucesso!" : "Esta Suíte já foi cadastrada!";
        
        rotaMenu = "/Home/Suites";

        return resposta;
    }
    
    private string AcessarSuites(Suite novaSuite = null)
    {
        string resposta = "Suíte ainda não cadastrada!";
        
        rotaMenu = "/Home/Suites";

        if (novaSuite != null)
        {
            bool sucesso = Suites.TryGetValue(novaSuite.TipoSuite, out var registro);
            resposta = sucesso ? FormatarSuite(registro.Item1, registro.Item2) : 
                resposta;
            return resposta;
        } 
        
        resposta = Suites.Count > 0 ? "" : "Nenhuma Suíte cadastrada!";
        
        foreach (var (tipo, suite) in Suites)
        {
            resposta += FormatarSuite(suite.Item1, suite.Item2);
        }
        
        return resposta;
    }

    private string FormatarSuite(Suite suite, Reserva reserva)
    {
        string formatado = $"          Tipo: {suite.TipoSuite}" +
                           $"\n     Capacidade: {suite.Capacidade}" +
                           $"\nValor da Diária: {suite.ValorDiaria}" +
                           $"\n      Reservada: {(reserva != null ? "Sim": "Não")
                           }\n\n";

        return formatado;
    }

    private List<string> AcessarListaSuites()
    {
        string novaRota = $"{DateTime.Now.ToUniversalTime():dd_MM_yyyy-HH:mm:ss}";
        
        List<string> listaSuitesDisponiveis = new List<string>();
        listaSuitesDisponiveis.Add(novaRota);
        
        foreach (var (tipo, suite) in Suites)
        {
            if(suite.Item2 == null) listaSuitesDisponiveis.Add(tipo);
        }
        
        listaSuitesDisponiveis.Add("Voltar");
        
        rotaMenu +=  $"/{novaRota}";
        
        if (listaSuitesDisponiveis.Count < 3) rotaMenu = "/Home/Reservas";
        
        return listaSuitesDisponiveis;
    }
    
    private string AdicionarReserva(Reserva novaReserva)
    {
        foreach (var hospede in novaReserva.Hospedes)
        {
            AdicionarHospede(hospede, novaReserva);
        }

        Suites[novaReserva.Suite.TipoSuite] = new (novaReserva.Suite, novaReserva);
        
        Reservas.Enqueue(novaReserva);
        
        return "Reserva feita com sucesso!";
    }
    
    private string AcessarReservas(Reserva reserva = null)
    {
        string resposta = "Falha: Reserva ainda não registrada!";
        
        rotaMenu = "/Home/Reservas";

        if (reserva != null)
        {
            bool sucesso = Reservas.Contains(reserva);
            resposta = sucesso ? FormatarReserva(reserva) : resposta;
            return resposta;
        } 
        
        resposta = Suites.Count > 0 ? "" : "Nenhuma Reserva registrada!";
        
        foreach (var reserva_ in Reservas)
        {
            resposta += FormatarReserva(reserva_);
        }
        
        return resposta;
    }
    
    private string FormatarReserva(Reserva reserva)
    {
        string hospedes = "";

        foreach (var NomeCompleto in Hospedes.Keys)
        {
            hospedes += NomeCompleto + ',';
        }
        
        string formatado = $"         Hóspedes: {hospedes.TrimEnd(',')}" +
                           $"\n          Suite: {reserva.Suite.TipoSuite}" +
                           $"\nValor da Diária: {reserva.Suite.ValorDiaria}" +
                           $"\nDias de Reserva: {reserva.DiasReservados}" +
                           $"\n    Valor Total: {reserva.CalcularValorDiaria()}\n\n";

        return formatado;
    }
        
    private string extrairBD(string caminho)
    {
        string conteudoJson = "";

        try
        {
            conteudoJson = File.ReadAllText(caminho);
        }
        catch (FileNotFoundException)
        {
            File.WriteAllText(caminho, "");
        }
        
        return conteudoJson;
    }
    
    /*Cria e carrega o banco de dados, com a estrutura Database/ Hospedes.json
                                                                 Reservas.json
                                                                 Suites.json
    */
    private void CarregarBancoDeDados()
    {
        if (!Directory.Exists(PastaBancoDeDados))
        {
            Directory.CreateDirectory(PastaBancoDeDados);
        }
        
        string  jsonHospedes = extrairBD($"{PastaBancoDeDados}Hospedes.json");
        Hospedes = JsonConvert.DeserializeObject<Dictionary<string, (Pessoa, Reserva)>>(jsonHospedes);
        if (Hospedes == null) Hospedes = new Dictionary<string, (Pessoa, Reserva)>();
        
        string  jsonSuites = extrairBD($"{PastaBancoDeDados}Suites.json");
        Suites = JsonConvert.DeserializeObject<Dictionary<string,(Suite, Reserva)>>(jsonSuites);
        if (Suites == null) Suites = new Dictionary<string, (Suite, Reserva)>();
        
        string  jsonReservas = extrairBD($"{PastaBancoDeDados}Reservas.json");
        Reservas = JsonConvert.DeserializeObject<Queue<Reserva>>(jsonReservas);
        if (Reservas == null) Reservas = new Queue<Reserva>();
    }
    
    private void AtualizarBancoDeDados()
    {
        string  jsonHospedes = JsonConvert.SerializeObject(Hospedes, Formatting.Indented);
        File.WriteAllText($"{PastaBancoDeDados}Hospedes.json", jsonHospedes);
        
        string  jsonSuites = JsonConvert.SerializeObject(Suites, Formatting.Indented);
        File.WriteAllText($"{PastaBancoDeDados}Suites.json", jsonSuites);
        
        string  jsonReservas = JsonConvert.SerializeObject(Reservas, Formatting.Indented);
        File.WriteAllText($"{PastaBancoDeDados}Reservas.json", jsonReservas);
    }
}
