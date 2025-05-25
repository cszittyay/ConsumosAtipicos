module AnalisisFrecuencial

open System.Numerics
open MathNet.Numerics
open MathNet.Numerics.IntegralTransforms

// Detectar patrón de consumo semanal utilizando la trsformada de Fourier FFT
// para determinar si el patrón de consumo es semanal, hay que analizar si la frecuencia dominante es la de la semana
// Se obtiene la frecuencia de la 'Semana' dividiendo la cantidad de muestra por 7
// Ej: si son 420 muestras, la semana tiene 420/7 = 60 es la frecuencia dominante


let detectarFrecuencias (serie: float[]) =
    let cbuffer = serie |> Array.map(fun x -> Complex(x, 0.0))
    Fourier.Forward(cbuffer, FourierOptions.Default)
    cbuffer |> Array.map(fun x -> x.Magnitude) 
            |> Array.toList 
            |> List.take 300 
            |> List.mapi(fun i x -> i, x / 10.0) 
            |> List.sortByDescending(fun x -> snd x)
            |> List.iter(fun (i,x) -> printfn "%d\t%f" i x )


