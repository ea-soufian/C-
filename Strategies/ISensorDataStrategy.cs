namespace PiBackend.Strategies

{
    public interface ISensorDataStrategy
    {
        // Methode om de ruwe binaire data om te zetten naar een gestandaardiseerde waarde
        double ConvertData(string data);
    }
}
