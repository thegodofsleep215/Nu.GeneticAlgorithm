namespace Nu.GeneticAlgorithm

type GeneticAlgorithm<'a>(population : 'a array, 
                            fitnessFunction : 'a -> double,
                            crossoverFunction : 'a -> 'a -> 'a, 
                            mutationFunction : 'a -> 'a,
                            mutationChance : double ) = 
    let mutable currentPopulation : ('a * double) array = population |> Array.map (fun x-> x, fitnessFunction(x))
    let rand : System.Random = new System.Random()
    let mutable lastBest = -2147483647.0
    let popSize = currentPopulation.Length

    member this.Run() : 'a =
        while(not this.IsDone) do
            let newPopulation = Async.Parallel [ for i in 0 .. currentPopulation.Length - 1 -> async { return this.ProcessNewChild(this.DoCrossover(currentPopulation)) } ] |> Async.RunSynchronously
            currentPopulation <- this.Cull(popSize, Array.append newPopulation currentPopulation)
        fst (currentPopulation |> Array.maxBy(fun x -> snd x))

    member this.RandomSelection(population : ('a * double) array) : 'a = 
        fst population.[rand.Next(0, population.Length)]

    member this.DoCrossover(population : ('a * double) array) : 'a = 
        let x = this.RandomSelection(population)
        let y = this.RandomSelection(population)
        crossoverFunction x y

    member this.ProcessNewChild(child: 'a) : ('a * double) = 
        let mc = rand.NextDouble()
        let result = if mc <= mutationChance then mutationFunction(child) else child
        result, fitnessFunction(result)

    member this.AverageFitness (population : ('a * double) array) : double =
            (Array.fold (fun acc x -> acc + snd x) 0.0 population)/double(population.Length)

    member this.Cull(size : int, population: ('a * double) array) : ('a * double) array =
        population |> Array.sortByDescending (fun (_, x) -> x) |> Array.take popSize

    member this.IsDone : bool = 
        let average = this.AverageFitness(currentPopulation)
        let goodOnes : double = double((currentPopulation |> Array.filter (fun (_, x) -> System.Math.Round(x,9) >= System.Math.Round(average,9))).Length)
        goodOnes/double(currentPopulation.Length) >= 0.9
