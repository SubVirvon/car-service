using System;
using System.Collections.Generic;
using System.Linq;

namespace Car_Service2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            Console.CursorVisible = false;
            Controller controller = new Controller(random, 15000);

            controller.Work();
        }
    }

    class Controller
    {
        private int _money;
        private Storage _storage;

        public Controller(Random random, int money)
        {
            _money = money;
            _storage = new Storage(random);
        }

        public void Work()
        {
            bool isWork = true;
            string[] commands = new string[] { "Продолжить", "Отказаться" };

            while (isWork)
            {    
                Client client = new Client(_storage.GetDetail());

                Console.SetCursorPosition(0, 5);
                Console.WriteLine($"Требует замены: {client.BrokenDetail.Name}\n");

                _storage.ShowAvailableDetailsInfo(cursorPositionY: Console.CursorTop);

                Console.WriteLine($"\nВаш счет: {_money}");

                ShowCommands(commands);

                if(ChooseElement(commands.Length) == 0)
                {
                    СontinueService(client);
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Клиент ушел");
                    Console.ReadKey();
                }

                if(_storage.GetAvailableDetailsCount() == 0)
                {
                    ShowFinishText($"У вас закончились детали для ремонта\nВаш счет: {_money} рублей");
                    isWork = false;
                }
                else if(_money <= 0)
                {
                    ShowFinishText("У вас не хватает денег. Вы закрылись");
                    isWork = false;
                }

                Console.Clear();
            }
        }

        private void ShowFinishText(string text)
        {
            Console.Clear();
            Console.WriteLine(text);
            Console.ReadKey();
        }

        private void СontinueService(Client client)
        {
            Console.Clear();

            _storage.ShowAvailableDetailsInfo(cursorPositionX: 2);

            Console.WriteLine($"\nТребует замены: {client.BrokenDetail.Name}");

            Detail selectedDetail = _storage.GetDetail(ChooseElement(_storage.GetAvailableDetailsCount()));
            int workCost = selectedDetail.Cost / 2;
            int result = 0;

            Console.Clear();

            if (selectedDetail.Equals(client.BrokenDetail))
            {
                result += workCost + selectedDetail.Cost;
                

                Console.WriteLine($"Вы заработали {result} рублей\nСтоимость замены: {workCost}\nСтоимость детали: {selectedDetail.Cost}");
            }
            else
            {
                result -= workCost;

                Console.WriteLine($"Вы заменили не сломанную деталь. Штраф {Math.Abs(result)} рублей");
            }

            _money += result;
            _storage.RemoveDetail(selectedDetail);

            Console.WriteLine($"Со склада удалена деталь: {selectedDetail.Name}");
            Console.ReadKey();
        }

        private void ShowCommands(string[] commands)
        {
            for(int i = 0; i < commands.Length; i++)
            {
                Console.SetCursorPosition(2, i);
                Console.WriteLine(commands[i]);
            }
        }

        private int ChooseElement(int ElementsCount)
        {
            const ConsoleKey Up = ConsoleKey.UpArrow;
            const ConsoleKey Down = ConsoleKey.DownArrow;
            const ConsoleKey Enter = ConsoleKey.Enter;
            int positionY = 0;
            bool isChosen = false;
            int result = 0;

            Console.SetCursorPosition(0, 0);
            Console.Write(">");

            while (isChosen == false)
            {
                if (positionY > ElementsCount - 1)
                    positionY = 0;
                else if (positionY < 0)
                    positionY = ElementsCount - 1;

                for (int i = 0; i < ElementsCount; i++)
                {
                    Console.SetCursorPosition(0, i);
                    Console.Write(" ");
                }

                Console.SetCursorPosition(0, positionY);
                Console.Write(">");

                switch (Console.ReadKey().Key)
                {
                    case Up:
                        positionY--;
                        break;
                    case Down:
                        positionY++;
                        break;
                    case Enter:
                        result = positionY;
                        isChosen = true;
                        break;
                }
            }

            return result;
        }
    }

    class Storage
    {
        private Random _random;
        private Detail[] _detailsVariants;
        private List<Cell> _availableDetails;

        public Storage(Random random)
        {
            _random = random;
            _detailsVariants = new Detail[] { new Detail("двигатель", 20800), new Detail("свечи", 1070), new Detail("резина", 6200), new Detail("боковое стекло", 1420), new Detail("лобовое стекло", 3600), new Detail("заднее стекло", 2450), new Detail("боковая дверь", 6830), new Detail("капот", 9800), new Detail("спойлер", 3990), new Detail("зеркало заднего вида", 1230) };
            _availableDetails = new List<Cell>();

            int minDetailCount = 1;
            int maxDetailCount = 5;

            foreach(var detail in _detailsVariants)
            {
                int detailCount = random.Next(minDetailCount, maxDetailCount + 1);

                _availableDetails.Add(new Cell(detail, detailCount));
            }
        }

        public void RemoveDetail(Detail detail)
        {
            Cell desiredCell = _availableDetails.Where(cell => cell.Detail.Equals(detail)).First();

            desiredCell.TakeOneDetail();

            if (desiredCell.Count <= 0)
                _availableDetails.Remove(desiredCell);
        }

        public int GetAvailableDetailsCount()
        {
            return _availableDetails.Count;
        }

        public Detail GetDetail()
        {
            return _detailsVariants[_random.Next(_detailsVariants.Length)];
        }

        public Detail GetDetail(int index)
        {
            return _availableDetails.ElementAt(index).Detail;
        }

        public void ShowAvailableDetailsInfo(int cursorPositionY = 0, int cursorPositionX = 0)
        {
            foreach (var cell in _availableDetails)
            {
                Detail detail = cell.Detail;

                Console.SetCursorPosition(cursorPositionX, cursorPositionY);
                Console.WriteLine($"{detail.Name} - {detail.Cost} рублей ({cell.Count} шт.)");

                cursorPositionY++;
            }
        }
    }

    class Cell
    {
        public Detail Detail { get; private set; }
        public int Count { get; private set; }

        public Cell(Detail detail, int count)
        {
            Detail = detail;
            Count = count;
        }

        public void TakeOneDetail()
        {
            Count--;
        }
    }

    class Client
    {
        public Detail BrokenDetail { get; private set; }

        public Client(Detail brokenDetail)
        {
            BrokenDetail = brokenDetail;
        }
    }

    struct Detail
    {
        public string Name;
        public int Cost;

        public Detail(string name, int cost)
        {
            Name = name;
            Cost = cost;
        }
    }
}
