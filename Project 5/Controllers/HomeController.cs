using Project_5.Models;
using Project_5.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_5.Controllers
{
    public class HomeController : Controller
    {
        private string tspDirectory = AppContext.BaseDirectory + "TSPs\\";
        //new random variable to use for random mating and mutations
        Random random = new Random();

        public ActionResult Index()
        {
            //the view model that will hold and serve all of the information to the page
            var vm = new HomeIndexViewModel();

            //define the mutation rates
            var mutationRateA = 5;
            var mutationRateB = 12;
            var mutationRateC = 15;

            //for each file, generate a new problem section
            foreach (var file in Directory.GetFiles(tspDirectory))
            {
                //read coords from the provided file(s)
                var coords = ReadCoordsFromFile(file);

                //randomly generate 4 parent paths that will be used for breeding for the problems
                var parentPaths = GenerateRandomPaths(coords);

                //generate the paths through genetic algorithm
                var tspProblemsForFile = new List<TSPProblemModel>();
                tspProblemsForFile.Add(GenerateTSPProblem(coords, parentPaths, "Mating A & Mutation A", "A", mutationRateA));
                tspProblemsForFile.Add(GenerateTSPProblem(coords, parentPaths, "Mating A & Mutation B", "A", mutationRateB));
                tspProblemsForFile.Add(GenerateTSPProblem(coords, parentPaths, "Mating A & Mutation C", "A", mutationRateC));
                tspProblemsForFile.Add(GenerateTSPProblem(coords, parentPaths, "Mating B & Mutation A", "B", mutationRateA));
                tspProblemsForFile.Add(GenerateTSPProblem(coords, parentPaths, "Mating B & Mutation B", "B", mutationRateB));
                tspProblemsForFile.Add(GenerateTSPProblem(coords, parentPaths, "Mating B & Mutation C", "B", mutationRateC));
                tspProblemsForFile.Add(GenerateTSPProblem(coords, parentPaths, "Mating C & Mutation A", "C", mutationRateA));
                tspProblemsForFile.Add(GenerateTSPProblem(coords, parentPaths, "Mating C & Mutation B", "C", mutationRateB));
                tspProblemsForFile.Add(GenerateTSPProblem(coords, parentPaths, "Mating C & Mutation C", "C", mutationRateC));

                //add in the wisdom of the crowds TSP problem
                vm.Problems.Add(WisdomOfTheCrowdsTSPProblem(coords, tspProblemsForFile.Select(x=>x.Path).ToList(), Path.GetFileName(file) + " Wisdom of the Crowds approach"));

                //add in the problems after the wisdom of the crowds problem
                vm.Problems.AddRange(tspProblemsForFile);
            }

            return View(vm);
        }

        //generate 4 random paths that will be the parents of all of the mutations
        private List<List<int>> GenerateRandomPaths(List<Coordinate> coords)
        {
            //list of full paths that will be returned
            var parentPaths = new List<List<int>>();

            for (int i = 0; i < 6; i++)
            {
                var paths = new List<int>();

                //keep track of all unused coord ids
                var unusedCoordIds = coords.Select(x => x.Id).ToList();

                //iterate through the amount of coords
                for (int y = 0; y < coords.Count; y++)
                {
                    //randomly generate next node
                    var nextPathId = unusedCoordIds[random.Next(unusedCoordIds.Count)];

                    paths.Add(nextPathId);

                    //remove the new id from the unused list
                    unusedCoordIds.Remove(nextPathId);
                }

                parentPaths.Add(paths);
            }
            return parentPaths;
        }

        private TSPProblemModel GenerateTSPProblem(List<Coordinate> coords, List<List<int>> parentPaths, string problemName, string matingMethod, int mutationRate)
        {
            //trackTimeToRun
            DateTime startTime = DateTime.Now;
            //generate tsp problem that will be displayed on the page
            var tspProblem = new TSPProblemModel();
            tspProblem.FileName = problemName;
            tspProblem.Coords = coords;

            //assign parentPaths to the variable that will hold each generation of paths
            var currentGenerationPaths = new List<List<int>>();
            //adding in paths through foreach loop so it has no reference to the parent paths and does not modify those
            foreach (var parent in parentPaths)
            {
                var newPath = new List<int>();
                foreach (var path in parent)
                    newPath.Add(path);
                currentGenerationPaths.Add(newPath);
            }

            //order the paths in ascending distance order. Shortest distance is the most fit parent to reproduce
            currentGenerationPaths = currentGenerationPaths.OrderBy(x => CalculateOverallDistance(x, coords)).ToList();

            //go through 5000 generations to mutate and mate the best routes
            for (int i = 0; i < 500; i++)
            {
                //variable to hold all of the children paths
                var childPaths = new List<List<int>>();

                //select provided mating method
                switch (matingMethod)
                {
                    case "A":
                        //best path mates with the second, third, and fourht best path on both sides and worst two paths are ditched
                        childPaths.Add(MatePaths(currentGenerationPaths[0], currentGenerationPaths[1], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[0], currentGenerationPaths[2], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[0], currentGenerationPaths[3], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[1], currentGenerationPaths[0], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[2], currentGenerationPaths[0], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[3], currentGenerationPaths[0], coords, mutationRate));
                        break;
                    case "B":
                        //top 4 paths mate together randomly
                        childPaths.Add(MatePaths(currentGenerationPaths[random.Next(0, 4)], currentGenerationPaths[random.Next(0, 4)], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[random.Next(0, 4)], currentGenerationPaths[random.Next(0, 4)], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[random.Next(0, 4)], currentGenerationPaths[random.Next(0, 4)], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[random.Next(0, 4)], currentGenerationPaths[random.Next(0, 4)], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[random.Next(0, 4)], currentGenerationPaths[random.Next(0, 4)], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[random.Next(0, 4)], currentGenerationPaths[random.Next(0, 4)], coords, mutationRate));
                        break;
                    case "C":
                        //best path mates randomly with the other paths
                        childPaths.Add(MatePaths(currentGenerationPaths[0], currentGenerationPaths[random.Next(1, 5)], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[0], currentGenerationPaths[random.Next(1, 5)], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[0], currentGenerationPaths[random.Next(1, 5)], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[random.Next(1, 5)], currentGenerationPaths[0], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[random.Next(1, 5)], currentGenerationPaths[0], coords, mutationRate));
                        childPaths.Add(MatePaths(currentGenerationPaths[random.Next(1, 5)], currentGenerationPaths[0], coords, mutationRate));
                        break;
                    default:
                        break;
                }

                //make a new list for child paths to mutate. Doing this because otherwise, it will run the mating and mutating at the same time and cause a headache that will take you about 8 - 10 hours to figure out no reason to bring this up
                var childPathsForMutations = new List<List<int>>();
                foreach (var child in childPaths)
                {
                    var newPath = new List<int>();
                    foreach (var path in child)
                        newPath.Add(path);
                    childPathsForMutations.Add(newPath);
                }

                //foreach path in the childPaths, apply the random mutation chances
                foreach (var childPath in childPathsForMutations)
                    MutatePath(childPath, mutationRate);

                //overwrite the currentGenerationPath with the childPaths before the next generation starts
                currentGenerationPaths = childPathsForMutations.OrderBy(x => CalculateOverallDistance(x, coords)).ToList();
            }

            //assign the path to the most fit path
            tspProblem.Path = currentGenerationPaths[0];
            //calculate overall distance
            tspProblem.TotalDistance = CalculateOverallDistance(tspProblem.Path, coords);
            //calculate time to run
            tspProblem.MillisecondsToRun = (DateTime.Now - startTime).TotalMilliseconds;

            return tspProblem;
        }

        //calculate total distance of the path
        private double CalculateOverallDistance(List<int> path, List<Coordinate> coords)
        {
            //track overall distance
            double overallDistance = 0;

            //loop through entire path and add the distance between the current city and the previous city
            for (int i = 1; i < path.Count; i++)
                overallDistance += DistanceBetween(coords.First(x => x.Id == path[i]), coords.First(x => x.Id == path[i - 1]));

            //add the distance between the last and first city
            overallDistance += DistanceBetween(coords.First(x => x.Id == path[path.Count - 1]), coords.First(x => x.Id == path[0]));

            return overallDistance;
        }


        //mate two full paths together
        private List<int> MatePaths(List<int> path1, List<int> path2, List<Coordinate> coords, int mutationRate)
        {
            //new variable to hold the path from mating
            var childPath = new List<int>();

            //add the first node from the second path as the first path. Doing it this way so it has no reference to the last one
            childPath.Add(path2[0]);

            //loop over the length of the path
            for (int i = 1; i < path1.Count; i++)
            {
                //select which path to use. Use path 1 with path 2 as failover if it's an odd number, otherwise use path 2 with path 1 as failover
                var pathToUse = path1;
                var backupPath = path2;
                if (i % 2 != 0)
                {
                    pathToUse = path2;
                    backupPath = path1;
                }

                //find where the last oath goes to on the path to use
                var lastCityIndex = pathToUse.FindIndex(x => x == childPath[childPath.Count - 1]);
                int nextCity = 0;

                //if that's the last path, go to the first element, otherwise use the next element
                if(lastCityIndex == pathToUse.Count - 1)
                    nextCity = pathToUse[0];
                else
                    nextCity = pathToUse[lastCityIndex + 1];
                //if the new city already exists in the current path, we need to try other methods
                if (childPath.Any(x => x == nextCity))
                {
                    //if that's the first path, go to the last element, otherwise use the previous element
                    if (lastCityIndex == 0)
                        nextCity = pathToUse[pathToUse.Count-1];
                    else
                        nextCity = pathToUse[lastCityIndex - 1];
                    //if the new city already exists in the current path, we need to try other methods
                    if (childPath.Any(x => x == nextCity))
                    {
                        //use the failover path
                        lastCityIndex = backupPath.FindIndex(x => x == childPath[childPath.Count - 1]);
                        //if that's the last path, go to the first element, otherwise use the next element
                        if (lastCityIndex == pathToUse.Count - 1)
                            nextCity = pathToUse[0];
                        else
                            nextCity = pathToUse[lastCityIndex + 1];

                        //can't use any of the existing connections. Have to combine random ones at this point
                        if (childPath.Any(x => x == nextCity))
                        {
                            //find all unused coords in the path
                            var unusedCoords = path1.Except(childPath).ToList();

                            //select a random city and add this to the path
                            nextCity = unusedCoords[random.Next(unusedCoords.Count())];
                        }
                    }
                }

                //add new step to child path
                childPath.Add(nextCity);
            }

            return childPath;
        }

        //apply the mutation rate to each NodePath in the path
        private void MutatePath(List<int> path, int mutationRate)
        {
            //go through each path step
            for (int x = 0; x < path.Count; x++)
            {
                //execute ranom percentage chance by generating a number 1-99. If it's less than or equal to the mutation rate, Mutate
                if (random.Next(1, 100) <= mutationRate)
                {//mutating is moving a city to a random part of the path
                    //keep track of the nodeIds
                    var mutatePoint = path[x];

                    //remove the point
                    path.Remove(mutatePoint);
                    //add the point in somewhere random
                    path.Insert(random.Next(0, path.Count),mutatePoint);
                }
            }
        }

        private TSPProblemModel WisdomOfTheCrowdsTSPProblem(List<Coordinate> coords, List<List<int>> paths, string problemName)
        {
            //generate tsp problem that will be displayed on the page
            var tspProblem = new TSPProblemModel();
            tspProblem.FileName = problemName;
            tspProblem.Coords = coords;

            //declare path that will be returned
            var newPath = new List<int>();

            //add in the first city from the shortest of the provided paths
            newPath.Add(paths.OrderBy(x => CalculateOverallDistance(x, coords)).ToList()[0][0]);

            //track cities that aren't in the path yet
            var unusedCoords = paths[0].Select(x => x).Except(newPath).ToList();

            //for the length of the first path, we will generate the wisdom of the crowds path
            for (int i = 1; i < paths[0].Count; i++)
            {
                //get last city in the new path
                var lastCity = newPath[newPath.Count - 1];

                //track all cities connected to the last city in all other paths
                var connectedCities = new List<int>();

                foreach(var path in paths)
                {
                    //find the index of the last city in the current path
                    var pathLastCityIndex = path.FindIndex(x => x == lastCity);
                    //add the city after the last city
                    connectedCities.Add(pathLastCityIndex == path.Count - 1 ? path[0] : path[pathLastCityIndex + 1]);
                    //add the city before the last city
                    connectedCities.Add(pathLastCityIndex == 0 ? path[path.Count - 1] : path[pathLastCityIndex - 1]);
                }
                var mostPopularConnections = connectedCities.GroupBy(s => s).OrderByDescending(g => g.Count()).ToList();
                var addedNewCity = false;
                foreach(var popularConnection in mostPopularConnections)
                {
                    if (unusedCoords.Contains(popularConnection.Key))
                    {
                        newPath.Add(popularConnection.Key);
                        unusedCoords.Remove(popularConnection.Key);
                        addedNewCity = true;
                        break;
                    }
                }

                if (!addedNewCity)
                {
                    var newCity = unusedCoords[random.Next(unusedCoords.Count)];
                    newPath.Add(newCity);
                    unusedCoords.Remove(newCity);
                }

            }

            tspProblem.Path = newPath;

            tspProblem.TotalDistance = CalculateOverallDistance(newPath, coords);

            return tspProblem;
        }

        //no longer used but keeping if it's needed in future
        //private void CorrectMutationFlaws(List<NodePath> childPath)
        //{
        //    for (int i = 0; i < childPath.Count - 1; i++)
        //    {
        //        int lastIndex = i - 1;
        //        if (i == 0)
        //            lastIndex = childPath.Count - 1;

        //        if (childPath[i].FromNode != childPath[lastIndex].ToNode)
        //        {
        //            //keep track of the nodeIds
        //            var newToNode = childPath[lastIndex].FromNode;
        //            var newFromNode = childPath[lastIndex].ToNode;

        //            //flip the node values
        //            childPath[lastIndex].ToNode = newToNode;
        //            childPath[lastIndex].FromNode = newFromNode;
        //        }
        //    }
        //}

        //read the list of coordinates from the .TSP file
        private List<Coordinate> ReadCoordsFromFile(string file)
        {
            var coords = new List<Coordinate>();

            var lines = System.IO.File.ReadAllLines(file);

            //get all lines after the 7th line since that's where coordinates start
            for (var i = 7; i < lines.Count(); i++)
            {
                //split out line into coordinate class I created
                var coordsText = lines[i].Split(' ');
                coords.Add(new Coordinate() { Id = Convert.ToInt32(coordsText[0]), Latitude = Convert.ToDouble(coordsText[1]), Longitude = Convert.ToDouble(coordsText[2]) });
            }

            return coords;
        }

        //use distance formula to find distance between two points
        private static double DistanceBetween(Coordinate coord1, Coordinate coord2)
        {
            return Math.Sqrt(Math.Pow((coord2.Latitude - coord1.Latitude), 2) + Math.Pow((coord2.Longitude - coord1.Longitude), 2));
        }
    }
}