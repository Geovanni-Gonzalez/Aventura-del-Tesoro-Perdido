public class PrologAdapter
{
	private string host = "127.0.0.1";
	private int port = 5000;

	public string Consultar(string comando)
	{
		using (var client = new TcpClient(host, port))
		using (var stream = client.GetStream())
		using (var writer = new StreamWriter(stream))
		using (var reader = new StreamReader(stream))
		{
			writer.WriteLine(comando);
			writer.Flush();
			return reader.ReadLine(); // Respuesta: "ok" o "error"
		}
	}
}
