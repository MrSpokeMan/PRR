import matplotlib.pyplot as plt
import random

class City:
    def __init__(self, x, y):
        self.x = x
        self.y = y

class TSP:
    def __init__(self, n):
        self.cities = []
        
        for _ in range(n):
            self.cities.append(self.generate_position())
        
    def generate_position(self):
        return City(random.randint(0, 100), random.randint(0, 100))
    
    def calculate_distance(self, city1: City, city2: City):
        return ((city1.x - city2.x) ** 2 + (city1.y - city2.y) ** 2) ** 0.5
    
    def show(self):
        x_coords = [city.x for city in self.cities]
        y_coords = [city.y for city in self.cities]

        plt.scatter(x_coords, y_coords)
        plt.xlabel('X Coordinates')
        plt.ylabel('Y Coordinates')
        plt.title('TSP Cities')
        plt.show()
        
class Ant:
    def __init__(self, tsp: TSP):
        self.tsp = tsp
        self.path = []
        self.fenotype = float('inf')
    
    def __call__(self, *args, **kwds):
        self.generate_path()
        self.calculate_fenotype()
    
    def generate_path(self):
        self.path = random.sample(self.tsp.cities, len(self.tsp.cities))
        
    def calculate_fenotype(self):
        self.fenotype = 0
        
        for i in range(len(self.path) - 1):
            self.fenotype += self.tsp.calculate_distance(self.path[i], self.path[i + 1])
        
        self.fenotype += self.tsp.calculate_distance(self.path[-1], self.path[0])
        
class GA:
    def __init__(self):
        self.population = []
        
    def generate_population(self, tsp: TSP, n):
        for _ in range(n):
            ant = Ant(tsp)
            ant()
            self.population.append(ant)
            
    def select_parents(self):
        parents = random.sample(self.population, len(self.population) // 2)
        return parents
    
    def crossover(self, parents):
        child = Ant(parents[0].tsp)
        child.path = parents[0].path[:len(parents[0].path) // 2] + [city for city in parents[1].path if city not in parents[0].path[:len(parents[0].path) // 2]]
        return child
            
    def __call__(self, *args, **kwds):
        self.evolve()
        
    def evolve(self):
        parents = self.select_parents()
        child = self.crossover(parents)
        child()
        self.population.append(child)
        self.population = sorted(self.population, key=lambda x: x.fenotype)[:-2]
            
    def show(self):
        for color, ant in enumerate(self.population):
            print(ant.fenotype)
            x_coords = [city.x for city in ant.path] + [ant.path[0].x]
            y_coords = [city.y for city in ant.path] + [ant.path[0].y]

            plt.plot(x_coords, y_coords, marker='o', linestyle='-', label=f'Ant {color}')
            plt.legend()
            plt.xlabel('X Coordinates')
            plt.ylabel('Y Coordinates')
            plt.title('TSP Path')
            plt.show()
    
    
    
if __name__ == "__main__":
    tsp = TSP(10)        
    ga = GA()
    ga.generate_population(tsp, 1000)
    
    while len(ga.population) > 30:
        pass
    ga.show()