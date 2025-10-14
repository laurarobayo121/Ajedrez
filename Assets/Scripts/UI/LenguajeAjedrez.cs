using UnityEngine;

public static class LenguajeAjedrez
{
    // Convierte coordenadas (fila, columna) a notación algebraica
    public static string ANotacion(int fila, int columna, int filasTotales)
    {
        char letra = (char)('A' + columna);
        int numero = fila + 1; 
        return $"{letra}{numero}";
    }

    // Convierte notación algebraica a coordenadas (fila, columna)
    public static Vector2Int ACoordenadas(string notacion, int filasTotales)
    {
        if (string.IsNullOrEmpty(notacion) || notacion.Length < 2)
            return Vector2Int.zero;

        char letra = char.ToUpper(notacion[0]);
        if (letra < 'A' || letra > 'Z')
            return Vector2Int.zero;

        int columna = letra - 'A';
        if (!int.TryParse(notacion.Substring(1), out int numero))
            return Vector2Int.zero;

        int fila = numero - 1; 

        return new Vector2Int(fila, columna);
    }
}
