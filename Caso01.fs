module Caso01

open System
open System.Numerics
open System.Globalization


open DataUtils





// + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + 
// Ventana de tiempo para la detección de cambios


let max0 x = if x < 0.0 then 0.0 else x

//  Compara el consumo con el minimo y el máximo de la ventana, 
// wy devuelve true si el consumo está fuera de los límites permitidos por la tolerancia


let filtroTolerancia  (win:ventana) tolerancia =  
        // El delta es el 70% de la desviación estandard
        let delta =  (tolerancia * win.stdev) ** 0.6
        win.avg - delta, win.avg + delta

let getVentanaBase  (datos: consumoDiario list) windowSize retraso =
    let win =  datos |> List.windowed(windowSize) 
                   |> List.map(fun x -> 
                        {desde = x[0].diaGas;
                         hasta = x[windowSize-1].diaGas; 
                         min = x |> List.map(fun z -> z.consumo) |> List.min;
                         max = x |> List.map(fun z -> z.consumo) |> List.max;
                         avg = x |> List.averageBy(fun z -> z.consumo);
                         stdev = 
                            let avg = x |> List.averageBy(fun z -> z.consumo)
                            let varianza = x |> List.averageBy(fun z -> pown (z.consumo - avg) 2)
                            sqrt varianza   
                         })
                   |> List.take (List.length datos - windowSize - retraso )
    win

// Completar
let maxZ x = Math.Max(0.0, x)


      
let getVentana  (datos: consumoDiario list) windowSize tolerancia retraso =
    let toleranciaPorc = tolerancia / 100.0
    let win =  getVentanaBase datos windowSize retraso

    let inicial = List.init (windowSize + retraso) (fun x -> win[0].avg)
    let winMin = win |> List.map(fun x -> filtroTolerancia x tolerancia |> fst |> max0) |> List.append inicial 
    let winMax = win |> List.map(fun x -> filtroTolerancia x tolerancia |> snd ) |> List.append inicial

    let nuevaData = datos |> List.skip (windowSize + retraso)
    
    let cambios = win |> List.zip nuevaData 
                      |> List.filter(fun (x, y) -> let xmin, xmax = filtroTolerancia y tolerancia
                                                   x.consumo > xmax || x.consumo < xmin)
                      // Determinar la magnitud del cambio
                      |> List.map(fun (x, y) -> 
                            let xmin, xmax = filtroTolerancia y tolerancia
                            let lim = if x.consumo > xmax then xmax else xmin
                            {diaGas = x.diaGas; consumo = x.consumo; limite = lim})
    cambios, winMin, winMax


// Devuelve una lista de tuplas (tolerancia, cantidad de cambios)
// Explora la tolerancia desde 5% hasta 50%
let ajustarToleranciaPorBarrido datos windowSize retraso =
    [5.0 .. 2.5 .. 50.0]
    |> List.map (fun t -> 
        let cambios, _, _ = getVentana datos windowSize t retraso
        t, cambios.Length)
