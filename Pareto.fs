module Pareto


// Principio de Pareto (80/20)

open DataUtils

// Dividir el rango de consumo en 10 partes iguales
// y encontrar el 80% de los casos; los datos que quedan fuera del 80% son atípicos


let getPareto  (datos: consumoDiario list) (estratos:int) =
    let consumoMax = datos |> List.maxBy (fun x -> x.consumo) |> fun x -> x.consumo
    let consumoMin = datos |> List.minBy (fun x -> x.consumo) |> fun x -> x.consumo
    
    let rangoConsumo = consumoMax - consumoMin
    let paso = rangoConsumo / float estratos

    // Crear los límites de los intervalos
    let limites = [0 .. estratos] |> List.map (fun i -> consumoMin + float i * paso, consumoMin + float (i + 1) * paso)
    
    // Contar los casos en cada intervalo
    let conteos = limites |> List.map (fun (min, max) -> 
        datos |> List.filter (fun x -> x.consumo >= min && x.consumo < max) |> List.length)
    let histograma = conteos |> List.zip limites 
 
    // Normalizar los conteos
    let total = List.sum conteos
    let conteosNorm = conteos |> List.map (fun x -> float x / float total)
    

    // Calcular la probabilidad acumulada
    let probabilidadAcumulada = List.scan (+) 0.0 conteosNorm



    conteos, limites, probabilidadAcumulada


// Ordeno los valores y obtengo el minimo para el cual se tiene x% de los casos => limite inferior
// Ordeno los valore en forma decreciente y obtengo el maximo para el cual se tiene x% de los casos => limite superior
let getPareto2 (datos: consumoDiario list) (porcDesvio:float) =
    let casos = datos.Length
    let casosAtipicos = int (float casos * porcDesvio)
    let limiteInferior = datos |> List.sortBy (fun x -> x.consumo) |> fun x -> x[casosAtipicos].consumo
    let limiteSuperior = datos |> List.sortByDescending (fun x -> x.consumo) |> fun x -> x[casosAtipicos].consumo
 
    limiteInferior, limiteSuperior

    
let cambiosPareto (datos: consumoDiario list) inf sup =
    let cambios = datos |> List.filter(fun c -> c.consumo > sup || c.consumo < inf )
                        |> List.map(fun   x -> { diaGas = x.diaGas; consumo = x.consumo; limite = if x.consumo > sup then sup else inf})

    graficar datos cambios "Cambios Pareto"
