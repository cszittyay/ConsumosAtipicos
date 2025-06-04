module Caso01

open System
open System.Numerics
open System.Globalization


open DataUtils



// === PARTE 2: Detección de Cambios ===

let detectarCambios (datos: consumoDiario list) (windowSize: int) (umbral: float) =
    let n = datos.Length
    [windowSize .. n - windowSize - 1]
    |> List.choose (fun i ->
        let prev = datos.[i - windowSize .. i - 1] |> List.map (fun x -> x.consumo)
        let next_ = datos.[i .. i + windowSize - 1] |> List.map (fun x -> x.consumo)
        let avgPrev = List.average prev
        let avgNext = List.average next_
        let delta = abs (avgNext - avgPrev)
        if delta > umbral then Some (datos.[i].diaGas, delta)
        else None
    )


// + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + 
// Ventana de tiempo para la detección de cambios



let magnitudCambio (valor:float) (min:float) (max:float) tolerancia =
    if valor > max * (1.0 + tolerancia ) then (valor - max) / max * 100.0
    elif valor < min * (1.0 - tolerancia) && min > 0 then (valor - min) / min * 100.0
    else 0.


let magnitudCambioGJ (valor:float) (min:float) (max:float)  = 
    if valor > max then valor - max
    else  valor - min


let max0 x = if x < 0.0 then 0.0 else x

//  Compara el consumo con el minimo y el máximo de la ventana, 
// wy devuelve true si el consumo está fuera de los límites permitidos por la tolerancia
let filtro consumo min max tolerancia =  consumo > max * (1.0 + tolerancia) || consumo < min * (1.0 - tolerancia) 


let filtroGauss  (win:ventana) tolerancia =  
        let delta =  (tolerancia * win.stdev) ** 0.7
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
    let winMin = win |> List.map(fun x -> filtroGauss x tolerancia |> fst |> max0) |> List.append inicial 
    let winMax = win |> List.map(fun x -> filtroGauss x tolerancia |> snd ) |> List.append inicial

    let nuevaData = datos |> List.skip (windowSize + retraso)
    
    let cambios = win |> List.zip nuevaData 
                      |> List.filter(fun (x, y) -> let xmin, xmax = filtroGauss y tolerancia
                                                   x.consumo > xmax || x.consumo < xmin)
                      // Determinar la magnitud del cambio
                      |> List.map(fun (x, y) -> 
                            let lim = if x.consumo > y.max * (1.+ toleranciaPorc) then y.max else y.min
                            {diaGas = x.diaGas; consumo = x.consumo; limite = lim})
    cambios, winMin, winMax


let getVentanaGJ  (datos: consumoDiario list) windowSize toleranciaGJ retraso =
    
    let win =  getVentanaBase datos windowSize retraso
    
    let inicial = List.init (windowSize-1) (fun x -> win[0].avg)
    let winMin = win |> List.map(fun x -> Math.Max( 0.0 ,x.min - toleranciaGJ)) |> List.append inicial
    let winMax = win |> List.map(fun x -> x.max * + toleranciaGJ) |> List.append inicial
    // Completar con el ancho de la ventana
    
    let nuevaData = datos |> List.skip (windowSize + retraso)

    let cambios = win |> List.zip nuevaData 
                      |> List.filter(fun (x, y) -> x.consumo > y.max + toleranciaGJ || x.consumo < y.min - toleranciaGJ)
                      // Determinar la magnitud del cambio
                      |> List.map(fun (x, y) -> 
                            let lim = if x.consumo > y.max+toleranciaGJ then y.max else y.min
                            {diaGas = x.diaGas; consumo = x.consumo; limite = lim})
    cambios, winMin, winMax


