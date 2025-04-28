using System;
using System.Threading;
using System.Xml;

namespace PHILOSOPHERS_THREADS
{
    // Класс, представляющий столовый прибор (вилку)
    class DiningFork
    {
        private readonly Mutex _mutex = new Mutex();

        // Взять вилку (захватить мьютекс)
        public void PickUp()
        {
            _mutex.WaitOne();
        }

        // Положить вилку обратно (освободить мьютекс)
        public void PutDown()
        {
            _mutex.ReleaseMutex();
        }
    }

    // Класс, представляющий философа
    class Philosopher
    {
        private readonly int _id;
        private readonly DiningFork _leftFork;
        private readonly DiningFork _rightFork;
        private uint _mealsConsumed;
        private double _totalWaitingTime;
        private DateTime _waitStartTime;
        private bool _shouldStop;
        private readonly bool _isDebugMode;
        private readonly Random _random = new Random();

        // Основной цикл жизни философа
        public void Live()
        {
            while (!_shouldStop)
            {
                Contemplate();
                Dine();
            }
        }

        // Процесс размышления
        private void Contemplate()
        {
            if (_isDebugMode)
            {
                Console.WriteLine($"Философ {_id} размышляет...");
            }

            Thread.Sleep(_random.Next(50, 150));

            if (_isDebugMode)
            {
                Console.WriteLine($"Философ {_id} проголодался.");
            }

            _waitStartTime = DateTime.Now;
        }

        // Процесс приема пищи
        private void Dine()
        {
            // Философ пытается взять обе вилки
            AcquireForks();

            // Расчет времени ожидания
            _totalWaitingTime += (DateTime.Now - _waitStartTime).TotalMilliseconds;

            if (_isDebugMode)
            {
                Console.WriteLine($"Философ {_id} начинает трапезу.");
            }

            // Время приема пищи
            Thread.Sleep(_random.Next(50, 150));

            _mealsConsumed++;

            // Философ возвращает вилки
            ReleaseForks();
        }

        // Взятие вилок
        private void AcquireForks()
        {
            // Сначала берем левую вилку
            _leftFork.PickUp();
            if (_isDebugMode)
            {
                Console.WriteLine($"Философ {_id} взял левую вилку.");
            }

            // Затем правую вилку
            _rightFork.PickUp();
            if (_isDebugMode)
            {
                Console.WriteLine($"Философ {_id} взял правую вилку.");
            }
        }

        // Возвращение вилок
        private void ReleaseForks()
        {
            // Сначала кладем правую вилку
            _rightFork.PutDown();
            if (_isDebugMode)
            {
                Console.WriteLine($"Философ {_id} положил правую вилку.");
            }

            // Затем левую вилку
            _leftFork.PutDown();
            if (_isDebugMode)
            {
                Console.WriteLine($"Философ {_id} положил левую вилку.");
            }
        }

        public Philosopher(int id, DiningFork leftFork, DiningFork rightFork, bool debugMode)
        {
            _id = id;
            _leftFork = leftFork;
            _rightFork = rightFork;
            _isDebugMode = debugMode;
        }

        // Остановка работы философа
        public void Stop()
        {
            _shouldStop = true;
        }

        // Вывод статистики
        public void DisplayStatistics()
        {
            Console.WriteLine($"{_id}\t\t{_mealsConsumed}\t\t\t{_totalWaitingTime:F0} мс");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Загрузка параметров из XML-файла
                var config = LoadConfiguration("data.xml");

                Console.WriteLine($"Запуск симуляции для {config.PhilosophersCount} философов...");
                Console.WriteLine($"Длительность: {config.SimulationDurationMs} мс");
                Console.WriteLine($"Режим отладки: {(config.DebugMode ? "вкл" : "выкл")}\n");

                // Инициализация вилок
                var forks = InitializeForks(config.PhilosophersCount);

                // Создание философов
                var philosophers = CreatePhilosophers(config.PhilosophersCount, forks, config.DebugMode);

                // Запуск потоков
                var threads = StartPhilosopherThreads(philosophers);

                // Ожидание завершения симуляции
                Thread.Sleep(config.SimulationDurationMs);

                // Остановка философов
                StopPhilosophers(philosophers);

                // Ожидание завершения потоков
                WaitForThreadsCompletion(threads);

                // Вывод результатов
                DisplayResults(philosophers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        // Загрузка конфигурации из XML
        private static (int PhilosophersCount, bool DebugMode, int SimulationDurationMs) LoadConfiguration(string filePath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            return (
                int.Parse(xmlDoc.SelectSingleNode("configuration/philosophers_count").InnerText),
                bool.Parse(xmlDoc.SelectSingleNode("configuration/debug_mode").InnerText),
                int.Parse(xmlDoc.SelectSingleNode("configuration/simulation_duration_ms").InnerText)
            );
        }

        // Инициализация вилок
        private static DiningFork[] InitializeForks(int count)
        {
            var forks = new DiningFork[count];
            for (int i = 0; i < count; i++)
            {
                forks[i] = new DiningFork();
            }
            return forks;
        }

        // Создание философов
        private static Philosopher[] CreatePhilosophers(int count, DiningFork[] forks, bool debugMode)
        {
            var philosophers = new Philosopher[count];
            for (int i = 0; i < count; i++)
            {
                // Каждый философ получает левую и правую вилку
                philosophers[i] = new Philosopher(
                    id: i + 1,
                    leftFork: forks[(i + 1) % count],  // Круговая расстановка
                    rightFork: forks[i],
                    debugMode: debugMode
                );
            }
            return philosophers;
        }

        // Запуск потоков для философов
        private static Thread[] StartPhilosopherThreads(Philosopher[] philosophers)
        {
            var threads = new Thread[philosophers.Length];
            for (int i = 0; i < philosophers.Length; i++)
            {
                threads[i] = new Thread(philosophers[i].Live);
                threads[i].Start();
            }
            return threads;
        }

        // Остановка философов
        private static void StopPhilosophers(Philosopher[] philosophers)
        {
            foreach (var philosopher in philosophers)
            {
                philosopher.Stop();
            }
        }

        // Ожидание завершения потоков
        private static void WaitForThreadsCompletion(Thread[] threads)
        {
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        // Вывод результатов
        private static void DisplayResults(Philosopher[] philosophers)
        {
            Console.WriteLine("\nРезультаты симуляции:");
            Console.WriteLine("Философ\t\tПриемов пищи\t\tВремя ожидания");
            foreach (var philosopher in philosophers)
            {
                philosopher.DisplayStatistics();
            }
        }
    }
}