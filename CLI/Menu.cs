using System.Net.Security;
using System.Numerics;
using System.Xml.Xsl;
using DesafioProjetoHospedagem.Models;

namespace DesafioProjetoHospedagem.CLI;

public class Menu
{
    private Pessoa NovoHospede;
    private Suite NovaSuite;
    private Reserva NovaReserva;
    private Dictionary<string, List<string>> MapaDoMenu; 
    
    public Menu()
    {
        MapaDoMenu = new();
        
        List<string> principal = new();
        principal.Add("Home");
        principal.Add("Hospedes");
        principal.Add("Reservas");
        principal.Add("Suites");
        principal.Add("Sair");
        
        MapaDoMenu.Add("/Home", principal);
        
        List<string> hospedes = AdicionarSecao("Hospedes");
        MapaDoMenu.Add($"/Home/Hospedes", hospedes);
        
        List<string> reservas = AdicionarSecao("Reservas");
        MapaDoMenu.Add("/Home/Reservas", reservas);
        
        List<string> suites = AdicionarSecao("Suites");
        MapaDoMenu.Add("/Home/Suites", suites);
    }

    public void AdicionarRota(string rota, List<string> ListaSecao)
    {
        MapaDoMenu.Add(rota, ListaSecao);
    }
    
    public void RemoverRota(string rota)
    {
        MapaDoMenu.Remove(rota);
    }

    public void Resposta(string resposta, string rota)
    {
        Header(rota);
        Console.WriteLine(resposta);
        Console.WriteLine();
        Console.WriteLine("Aperte qualquer tecla para continuar...");
        Console.ReadKey();
    }

    private List<string> AdicionarSecao(string tipo)
    {
        List<string> secao = new();
        secao.Add(tipo);
        secao.Add($"Cadastrar_{tipo.TrimEnd('s')}");
        secao.Add($"Buscar_{tipo.TrimEnd('s')}");
        secao.Add($"Listar_{tipo}");
        secao.Add("Voltar");

        return secao;
    }

    public void Header(string rota)
    {
        Console.Clear();
        Console.CursorVisible = false;
        Console.WriteLine();
        Console.WriteLine("--------Sistema de Hospedagem--------");
        Console.WriteLine();
        Console.WriteLine($"{rota.TrimStart('/')}");
        Console.WriteLine();
    }
    
    public string Interface(string rota)
    {
        bool haOpcoes = MapaDoMenu.TryGetValue(rota, out List<string> opcoes);

        if (!haOpcoes || opcoes.Count == 0) return Operacoes(rota);
        
        Header(rota);
        Console.WriteLine("Escolha pressionando uma tecla:");
        Console.WriteLine();

        int index = 1;
        for (; index < opcoes.Count - 1; index++)
        {
            Console.WriteLine($"{index} - {opcoes[index].Replace('_', ' ')}");
        }
        
        Console.WriteLine();
        Console.WriteLine($"0 - {opcoes[index]}");
       
        int key = int.Parse($"{Console.ReadKey(true).KeyChar}");
        
        if (key < opcoes.Count)
        {
            if (key == 0) return Interface(rota.Replace($"/{opcoes[key]}", ""));
            
            return Interface($"{rota}/{opcoes[key]}");
        }
        
        return Interface(rota);
    }

    private string Operacoes(string rota)
    {
        switch (rota)
        {
            case "/Home/Hospedes/Cadastrar_Hospede":
            case "/Home/Hospedes/Buscar_Hospede": return Hospedes(rota);
            
            case "/Home/Suites/Cadastrar_Suite":
            case "/Home/Suites/Buscar_Suite": return Suites(rota);
            
            case "": return null;
        }
        
        return rota;
    }
    
    private string Hospedes(string rota)
    {
        Header(rota);
        Console.WriteLine("Preencha os campos:");
        Console.WriteLine();
        
        NovoHospede = PreencherHospede();
        
        return rota;
    }

    private Pessoa PreencherHospede()
    {
        Console.Write("     Nome: ");
        var cursorNome = Console.GetCursorPosition();
        Console.WriteLine();
        
        Console.Write("Sobrenome: ");
        var cursorSobrenome = Console.GetCursorPosition();
        Console.WriteLine();
       
        Console.SetCursorPosition(cursorNome.Left, cursorNome.Top);
        string nome = ValidarString();
        
        Console.SetCursorPosition(cursorSobrenome.Left, cursorSobrenome.Top);
        string sobrenome = ValidarString();
        
        return new(nome, sobrenome);
    }
    
    private string Suites(string rota)
    {
        Header(rota);
        Console.WriteLine("Preencha os campos:");
        Console.WriteLine();
        
        Console.Write("           Tipo: ");
        var cursorTipo = Console.GetCursorPosition();
        Console.WriteLine();
        
        var cursorCapacidade = Console.GetCursorPosition();
        Console.Write("     Capacidade: ");
        Console.WriteLine();
        
        Console.Write("Valor da Diária: ");
        var cursorDiaria = Console.GetCursorPosition();
        Console.WriteLine();
        
        Console.SetCursorPosition(cursorTipo.Left, cursorTipo.Top);
        string tipo = ValidarString();
        
        Console.SetCursorPosition(cursorCapacidade.Left, cursorCapacidade.Top);
        int capacidade;
        
        do
        {
            Console.SetCursorPosition(cursorCapacidade.Left, cursorCapacidade.Top);
            Console.Write("     Capacidade:  ");
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            capacidade = ValidarInt();
        } 
        while (capacidade < 2);
        
        Console.SetCursorPosition(cursorDiaria.Left, cursorDiaria.Top);
        decimal valorDiaria = validarDecimal();
        
        NovaSuite = new(tipo, capacidade, valorDiaria);
        
        return rota;
    }
    
    public void FormularioDeReservas(string rota, Suite suiteSelecionada)
    {
        Header(rota);
        Console.Write("Dias de Reserva: ");
        int diasReservados = ValidarInt();
        
        NovaReserva = new Reserva(diasReservados);
        NovaReserva.CadastrarSuite(suiteSelecionada);
        decimal valorTotal = NovaReserva.CalcularValorDiaria();
        
        Console.WriteLine();
        Console.WriteLine($"Valor total: {valorTotal:C}");
        Console.WriteLine();

        var position = Console.GetCursorPosition();
        int quantideHospedes;

        do
        {
            Console.SetCursorPosition(position.Left, position.Top);
            Console.Write($"Defina a quantidade de hóspedes(1 à {suiteSelecionada.Capacidade}):  ");
            Console.SetCursorPosition(Console.CursorLeft - 1, position.Top);
            quantideHospedes = ValidarInt();
        } 
        while (quantideHospedes < 0 || quantideHospedes > suiteSelecionada.Capacidade);
        
        List<Pessoa> hospedesDaReserva = new();

        for (int i = 1; i <= quantideHospedes; i++)
        {
            Console.WriteLine();
            Console.WriteLine($"-----Hóspede {i}-----");
            hospedesDaReserva.Add(PreencherHospede());
        }
        
        NovaReserva.CadastrarHospedes(hospedesDaReserva);
    }

    private string ValidarString()
    {
        string entrada;
        var position = Console.GetCursorPosition();
        Console.CursorVisible = true;
        do
        {
            Console.SetCursorPosition(position.Left, position.Top);
            entrada = Console.ReadLine();
        } 
        while (string.IsNullOrEmpty(entrada));

        return entrada;
    }
    
    private int ValidarInt()
    {
        int entrada;

        var position = Console.GetCursorPosition();
        Console.CursorVisible = true;
        do
        {
            Console.SetCursorPosition(position.Left, position.Top);
            int.TryParse(Console.ReadLine(), out entrada);
        }
        while (entrada == 0);

        return entrada;
    }
    
    private decimal validarDecimal()
    {
        decimal entrada;

        var position = Console.GetCursorPosition();
        Console.CursorVisible = true;
        do
        {
            Console.SetCursorPosition(position.Left, position.Top);
            decimal.TryParse(Console.ReadLine(), out entrada);
        }
        while (entrada == 0m);

        return entrada;
    }
                
    public void Deconstruct
    (
        out Pessoa novoHospede,
        out Suite novaSuite,
        out Reserva novaReserva
    )
    {
        novoHospede = NovoHospede;
        novaSuite = NovaSuite;
        novaReserva = NovaReserva;
    }
}