
using System.Text.Json;

public class Usuario
{
    // Propiedad estática para llevar la cuenta del último ID asignado
    private static int ultimoId = 0;
    public int id { get; set; }
    public string Nombre { get; set; }
    public bool EsJubilado { get; set; }
    public decimal Saldo { get; set; }
    public decimal LimiteExtraccion { get; set; }

    // Constructor de la clase Usuario
    public Usuario(string nombre, bool esJubilado)
    {
        this.id = ++ultimoId;
        Nombre = nombre;
        EsJubilado = esJubilado;
        Saldo = 0;
        LimiteExtraccion = esJubilado ? 10000 : 20000;
    }
}

public class Operacion
{
    public DateTime Fecha { get; set; }
    public string Cajero { get; set; }
    public decimal Monto { get; set; }
    public string Tipo { get; set; }
    public int idUsuario { get; set; }

    // Constructor de la clase Operacion
    public Operacion(DateTime fecha, string cajero, decimal monto, int idusuario, string tipo)
    {
        Fecha = fecha;
        Cajero = cajero;
        Monto = monto;
        idUsuario = idusuario;
        Tipo = tipo;
    }
}

public class CajeroAutomatico
{
    private List<Usuario> usuarios;
    private List<Operacion> operaciones;
    private string nombre { get; set; }
    private string usuarioActual;
    private const string archivoUsuarios = "usuarios.json";
    private const string archivoOperaciones = "operaciones.json";

    // Constructor de la clase CajeroAutomatico
    public CajeroAutomatico()
    {
        usuarios = new List<Usuario>();
        operaciones = new List<Operacion>();
        nombre = "CAJERO - 1";
        CargarUsuarios(); // Carga los usuarios y operaciones guardados en archivos
    }

    // Carga los usuarios y operaciones guardados en archivos JSON
    private void CargarUsuarios()
    {
        if (File.Exists(archivoUsuarios))
        {
            // Lee el contenido de los archivos y los deserializa
            string json = File.ReadAllText(archivoUsuarios);
            // Convierte el JSON a una lista de objetos Usuario
            usuarios = JsonSerializer.Deserialize<List<Usuario>>(json);
            // Lee el contenido de los archivos y los deserializa
            string jsonOperaciones = File.ReadAllText(archivoOperaciones);
            // Convierte el JSON a una lista de objetos Operacion
            operaciones = JsonSerializer.Deserialize<List<Operacion>>(jsonOperaciones);
        }
    }

    // Guarda los usuarios y operaciones en archivos JSON
    private void GuardarUsuarios()
    {
        // Serializa la lista de usuarios a JSON
        string json = JsonSerializer.Serialize(usuarios);
        // Guarda el JSON en un archivo
        File.WriteAllText(archivoUsuarios, json);
        // Serializa la lista de operaciones a JSON
        string jsonOperaciones = JsonSerializer.Serialize(operaciones);
        File.WriteAllText(archivoOperaciones, jsonOperaciones);
    }

    // Verifica si un usuario cumple con las condiciones para ofrecerle un crédito pre acordado
    private void VerificarCreditoPreAcordado(Usuario usuario)
    {
        // Verifica si el saldo del usuario ha sido positivo durante dos meses consecutivos
        int mesesConSaldoPositivo = 0;
        DateTime fechaActual = DateTime.Now;

        // Recorre las operaciones del usuario en orden inverso para verificar los dos meses más recientes
        for (int i = operaciones.Count - 1; i >= 0; i--)
        {
            Operacion operacion = operaciones[i];
            if (operacion.idUsuario == usuario.id && operacion.Monto > 20000)
            {
                if ((fechaActual - operacion.Fecha).TotalDays <= 30)
                {
                    mesesConSaldoPositivo++;
                    if (mesesConSaldoPositivo >= 2)
                    {
                      
                        usuario.LimiteExtraccion = 80000; // Establece el nuevo límite de extracción
                        Console.WriteLine("¡Felicidades! Se te ha ofrecido un crédito pre acordado de 80000 pesos.");
                        return;
                    }
                }
                else
                {
                    break; // Si la operación es más antigua de un mes, se detiene la verificación
                }
            }
        }
    }

    // Crea una cuenta de usuario y la agrega a la lista de usuarios
    public void CrearCuenta(string nombre, bool esJubilado)
    {

        // Verifica si ya existe un usuario con el mismo nombre
        if (usuarios.Exists(u => u.Nombre == nombre))
        {
            Console.WriteLine("Ya existe un usuario con ese nombre, porfavor intente denuevo con otro nombre.");
            return;
        }
        else
        {
            Console.WriteLine("Cuenta creada con éxito.");
            usuarios.Add(new Usuario(nombre, esJubilado));
            GuardarUsuarios(); // Guarda los cambios en el archivo
        }

    }

    // Inicia sesión de un usuario
    public bool IniciarSesion(string nombreUsuario)
    {
        // Verifica si el usuario existe en la lista de usuarios
        if (usuarios.Exists(u => u.Nombre == nombreUsuario))
        {
            Usuario usuario = usuarios.Find(u => u.Nombre == nombreUsuario);
            VerificarCreditoPreAcordado(usuario); // Verifica si se le debe ofrecer un crédito pre acordado
           

            usuarioActual = nombreUsuario;
            return true;
        }
        else
        {
            Console.WriteLine("Usuario no encontrado. Intente de nuevo.");
            return false;
        }
    }

    // Realiza un depósito en la cuenta del usuario actual y registra la operación
    public void Depositar(decimal monto, string cajero)
    {
        // Busca el usuario actual en la lista de usuarios
        var usuario = usuarios.Find(u => u.Nombre == usuarioActual);
        if (usuario != null)
        {
            usuario.Saldo += monto;
            operaciones.Add(new Operacion(DateTime.Now, cajero, monto, usuario.id, "DEPOSITO"));
            GuardarUsuarios(); // Guarda los cambios en el archivo
        }
    }

    // Realiza una extracción de dinero de la cuenta del usuario actual y registra la operación
    public void Extraer(decimal monto, string cajero)
    {
        // Busca el usuario actual en la lista de usuarios
        var usuario = usuarios.Find(u => u.Nombre == usuarioActual);
        if (usuario != null)
        {
            if (usuario.Saldo - monto >= -usuario.LimiteExtraccion)
            {
                usuario.Saldo -= monto;
                operaciones.Add(new Operacion(DateTime.Now, cajero, -monto, usuario.id, "EXTRACCION"));
                GuardarUsuarios(); // Guarda los cambios en el archivo
            }
            else
            {
                Console.WriteLine("Extracción no permitida. Saldo insuficiente.");
            }
        }
    }

    // Muestra el saldo actual del usuario
    public void MostrarSaldo()
    {
        // Busca el usuario actual en la lista de usuarios
        var usuario = usuarios.Find(u => u.Nombre == usuarioActual);
        if (usuario != null)
        {
            Console.WriteLine($"Saldo de {usuario.Nombre}: {usuario.Saldo}");
           if(usuario.LimiteExtraccion > 20000)
            {
                Console.WriteLine("=====================");
                Console.WriteLine($"Tenemos buenas noticias para vos!");
                Console.WriteLine($"Tu limite de extraccion ha sido aumentado a {usuario.LimiteExtraccion}");

            }  
        }
    }

    // Lista las operaciones realizadas por el usuario actual
    public void ListarOperaciones()
    {
        
        // Busca el usuario actual en la lista de usuarios
        Usuario usuario = usuarios.Find(u => u.Nombre == usuarioActual);
        int count = 0;

        if (usuario != null)
        {
            // Recorre la lista de operaciones y muestra las operaciones del usuario actual
            foreach (Operacion operacion in operaciones)
            {
                // Verifica si la operación pertenece al usuario actual
                if (operacion.idUsuario == usuario.id)
                {
                    Console.WriteLine($"Fecha: {operacion.Fecha} - TIPO: {operacion.Tipo} - Monto: {operacion.Monto} ");
                    count++;
                }
            }
            if (count == 0)
            {
                Console.WriteLine("No hay operaciones realizadas");
            }
        }
    }

    // Muestra el menú de operaciones disponibles para el usuario actual
    public void MostrarMenuOperaciones()
    {
        bool salir = false;
        while (!salir)
        {
            Console.Clear();
            Console.WriteLine($"Bienvenido, {usuarioActual.ToUpper()}.");
            Console.WriteLine("======= MENÚ =======");
            Console.WriteLine("1. Depositar");
            Console.WriteLine("2. Extraer");
            Console.WriteLine("3. Mostrar Saldo");
            Console.WriteLine("4. Listar Operaciones");
            Console.WriteLine("5. Salir");
            Console.WriteLine("=====================");
            Console.Write("Seleccione una opción: ");

            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    Console.Clear();
                    Console.WriteLine("======= DEPOSITAR =======");
                    Console.Write("Ingrese el monto a depositar: ");
                    decimal montoDeposito = Convert.ToDecimal(Console.ReadLine());
                    Depositar(montoDeposito, this.nombre);
                    Console.WriteLine("Depósito realizado con éxito.");
                    Console.WriteLine("===========================");
                    Console.WriteLine("Presione Enter para continuar...");
                    Console.ReadLine();
                    break;
                case "2":
                    Console.Clear();
                    Console.WriteLine("======= EXTRAER =======");
                    Console.Write("Ingrese el monto a extraer: ");
                    decimal montoExtraccion = Convert.ToDecimal(Console.ReadLine());

                    Extraer(montoExtraccion, this.nombre);
                    Console.WriteLine("=======================");
                    Console.WriteLine("Presione Enter para continuar...");
                    Console.ReadLine();
                    break;
                case "3":
                    Console.Clear();
                    Console.WriteLine("======= SALDO =======");
                    MostrarSaldo();
                    Console.WriteLine("=====================");
                  
                    Console.WriteLine("Presione Enter para continuar...");
                    Console.ReadLine();
                    break;
                case "4":
                    Console.Clear();
                    Console.WriteLine("======= OPERACIONES =======");
                    ListarOperaciones();
                    Console.WriteLine("===========================");
                    Console.WriteLine("Presione Enter para continuar...");
                    Console.ReadLine();
                    break;
                case "5":
                    salir = true;
                    break;
                default:
                    Console.WriteLine("Opción no válida. Por favor, seleccione una opción válida.");
                    Console.WriteLine("Presione Enter para continuar...");
                    Console.ReadLine();
                    break;
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        CajeroAutomatico cajero = new CajeroAutomatico();
      

        bool sesionIniciada = false;
        string nombreUsuario = "";

        while (!sesionIniciada)
        {
            Console.Clear();
            Console.WriteLine("======= MENÚ DE INICIO =======");
            Console.WriteLine("1. Iniciar sesión");
            Console.WriteLine("2. Crear cuenta");
            Console.WriteLine("3. Salir");
            Console.WriteLine("===============================");
            Console.Write("Seleccione una opción: ");

            string opcionInicio = Console.ReadLine();

            switch (opcionInicio)
            {
                case "1":
                    Console.Clear();
                    Console.Write("Ingrese su nombre de usuario: ");
                    nombreUsuario = Console.ReadLine();
                    if (cajero.IniciarSesion(nombreUsuario))
                    {
                        sesionIniciada = true;
                        cajero.MostrarMenuOperaciones();
                    }
                    else
                    {
                        Console.WriteLine("Usuario no encontrado. Intente de nuevo.");
                        Console.WriteLine("Presione Enter para continuar...");
                        Console.ReadLine();
                    }
                    break;
                case "2":
                    Console.Clear();
                    Console.Write("Ingrese su nombre de usuario: ");
                    string nuevoUsuario = Console.ReadLine();
                    Console.Write("¿Es jubilado? (S/N): ");
                    bool esJubilado = Console.ReadLine().ToUpper() == "S";
                    cajero.CrearCuenta(nuevoUsuario, esJubilado);

                    Console.WriteLine("Presione Enter para continuar...");
                    Console.ReadLine();
                    break;
                case "3":
                    // Sale del programa
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opción no válida. Por favor, seleccione una opción válida.");
                    Console.WriteLine("Presione Enter para continuar...");
                    Console.ReadLine();
                    break;
            }
        }
    }
}

