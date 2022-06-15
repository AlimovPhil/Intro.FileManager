using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Lesson9.Utils;

namespace Lesson9
{

    internal class Program
    {
        const int WINDOW_HEIGHT = 40; // Высота окна приложения;
        const int WINDOW_WIDTH = 120; // Ширина окна приложения
        private static string currentDir = Directory.GetCurrentDirectory();

        static void Main(string[] args)
        {
            currentDir = Properties.Settings.Default.LastPath; // Читаем директорию из конфига

            Console.Title = "File Manager"; // Название окна

            Console.BackgroundColor = ConsoleColor.Black; // Цвет окна
            Console.ForegroundColor = ConsoleColor.DarkGreen; // Цвет шрифта

            Console.SetBufferSize(WINDOW_WIDTH, WINDOW_HEIGHT);
            Console.SetWindowSize(WINDOW_WIDTH, WINDOW_HEIGHT);

            DrawWindow(0,0,WINDOW_WIDTH,26);
            DrawWindow(0, 26, WINDOW_WIDTH, 10);
            DrawTree(new DirectoryInfo(currentDir), 1);
            ShowDirInfo(currentDir);
            UpdateConsole();

            Console.ReadLine();
        }

        /// <summary>
        /// Получить текущую позицию курсора
        /// </summary>
        /// <returns></returns>
        static (int left, int top) GetCursorPosition()
        {
            return (Console.CursorLeft,
                    Console.CursorTop);
        }

        /// <summary>
        /// Обработка процесса ввода команды
        /// </summary>
        /// <param name="width">Длина строки ввода</param>
        static void ProcessEnterCommand(int width)
        {
            (int left, int top) = GetCursorPosition();
            StringBuilder command = new StringBuilder();
            ConsoleKeyInfo keyInfo;
            char key;

            do
            {
                keyInfo = Console.ReadKey();
                key = keyInfo.KeyChar;

                if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.UpArrow)
                    command.Append(key);

                (int currentLeft, int currentTop) = GetCursorPosition();

                if (currentLeft == width - 2)
                {
                    Console.SetCursorPosition(currentLeft - 1, top);
                    Console.Write(" ");
                    Console.SetCursorPosition(currentLeft - 1, top);
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (command.Length > 0)
                        command.Remove(command.Length - 1, 1);
                    if (currentLeft >= left)
                    {
                        Console.SetCursorPosition(currentLeft, top);
                        Console.Write(" ");
                        Console.SetCursorPosition(currentLeft, top);
                    }
                    else
                    {
                        command.Clear();
                        Console.SetCursorPosition(left, top);
                    }
                }

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    //ToDo....
                }

            } 
            while (keyInfo.Key != ConsoleKey.Enter);
            
            ParseCommandString(command.ToString());
        }

        /// <summary>
        /// Обработка введенной команды
        /// </summary>
        /// <param name="command">Введенная команда</param>
        static void ParseCommandString(string command)
        {
            string[] commandParams = command.ToLower().Split(' ');
            if (commandParams.Length > 0)
            {
                switch (commandParams[0])
                {
                    case "cd":              // смена каталога и вывод дерева этого каталога
                        if (commandParams.Length > 1)
                            if (Directory.Exists(commandParams[1]))
                            {
                                currentDir = commandParams[1];
                                Properties.Settings.Default.LastPath = currentDir;
                                DrawTree(new DirectoryInfo(commandParams[1]), 1);
                                ShowDirInfo(currentDir);
                                UpdateConsole();
                                Properties.Settings.Default.Save(); // при исполнении команды cd сохраняется путь каталога в файл конфиг
                            }
                        break;

                    case "ls":              // вывод дерева введеного каталога с указанием начальной страницы отображения
                        if (commandParams.Length > 1 && Directory.Exists(commandParams[1]))
                            if (commandParams.Length > 3 && commandParams[2] == "-p" && int.TryParse(commandParams[3], out int n))
                            {
                                DrawTree(new DirectoryInfo(commandParams[1]), n);
                                ShowDirInfo(commandParams[1]);
                            }
                            else
                            {
                                DrawTree(new DirectoryInfo(commandParams[1]), 1);
                                ShowDirInfo(commandParams[1]);
                            }
                        break;

                    case "cp":              // копирование каталога
                        if (commandParams.Length > 1 && Directory.Exists(commandParams[1]))
                            if (commandParams.Length > 2) 
                            {
                               CopyDirectory(commandParams[1], commandParams[2]);
                            }
                        break;

                    case "cf":              // копирование файла
                        if (commandParams.Length > 1)
                            if (commandParams.Length > 2 && commandParams.Length < 4)
                            {
                                CopyFile(commandParams[1], commandParams[2]);
                            }
                        break;

                    case "rf":              // удаление файла
                        if (commandParams.Length > 1 && commandParams.Length < 3)
                            RemoveFile(commandParams[1]);
                        break;

                    case "rm":              // удаление каталога
                        if (commandParams.Length > 1 && commandParams.Length < 3)
                                RemoveDir(commandParams[1]);
                        break;

                    case "file":              // вывод информации о файле
                        if (commandParams.Length > 1 && commandParams.Length < 3)
                            ShowFileInfo(commandParams[1]);
                        break;

                    case "dir":               // вывод информации о папке
                        if (commandParams.Length > 1 && commandParams.Length < 3)
                            ShowDirInfo(commandParams[1]);
                        break;

                    case "help":               // вывод доступных команд
                        if (commandParams.Length == 1)
                            ShowHelp();
                        break;

                    case "exit":            // закрытие приложения
                        Properties.Settings.Default.Save();
                        Environment.Exit(0);
                        break;
                }
            }
            UpdateConsole();
        }

        /// <summary>
        /// Вывод информации о папке
        /// </summary>
        /// <param name="dirPath">Путь к папке</param>
        static void ShowDirInfo(string dirPath)
        {
            DrawWindow(0, 26, WINDOW_WIDTH, 10);
            if (Directory.Exists(dirPath))
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                    string[] info = { $"Название папки: {dirInfo.Name.ToString()}",
                            $"Расположение: {dirInfo.FullName.ToString()}",
                            $"Создана: {dirInfo.CreationTime.ToString()}",
                            $"Изменена: {dirInfo.LastWriteTime.ToString()}",
                            };
                    for (int i = 0; i < info.Length; i++)
                    {
                        Console.SetCursorPosition(1, 27 + i);
                        Console.WriteLine(info[i]);
                    }
                    double folderSize = 0.0;                                                //Подсчет размера папки
                    FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                    foreach (FileInfo file in files) folderSize += file.Length;

                    (int currentLeft, int currentTop) = GetCursorPosition();                //Вывод в консоль размера папки
                    Console.SetCursorPosition(currentLeft+1, currentTop);
                    
                    //Конвертация размера папки в КБ и МБ
                    if (folderSize < 1024)
                    Console.WriteLine($"Размер папки: {folderSize} byte");
                    if (folderSize > 1023 && folderSize < 1048576)
                    {
                       double folderSizeKb = folderSize / 1024;
                       Console.WriteLine($"Размер папки: {Math.Round(folderSizeKb)} KB");
                    }
                    if (folderSize >= 1048576)
                    {
                        double folderSizeKb = folderSize / (1024*1024);
                        Console.WriteLine($"Размер папки: {Math.Round(folderSizeKb)} MB");
                    }
                }

                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }


        /// <summary>
        /// Вывод информации о файле
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        static void ShowFileInfo(string filePath)
        {
            DrawWindow(0, 26, WINDOW_WIDTH, 10);
            if (File.Exists(filePath))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    string[] info = { $"Название файла: {fileInfo.Name.ToString()}",
                            $"Расположение файла: {fileInfo.DirectoryName.ToString()}",
                            $"Расширение: {fileInfo.Extension.ToString()}",
                            $"Размер: {fileInfo.Length.ToString()} bytes",
                            $"Создан: {fileInfo.CreationTime.ToString()}",
                            $"Изменен: { fileInfo.LastWriteTime.ToString() }",
                            };
                    for (int i = 0; i < info.Length; i++)
                    {
                        Console.SetCursorPosition(1, 27 + i);
                        Console.WriteLine(info[i]);
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        /// <summary>
        /// Копирование всего каталога с вложенными папками и файлами
        /// </summary>
        /// <param name="sourceDir">Родительский каталог</param>
        /// <param name="destinationDir">Каталог назначения</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        static void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Собираем информацию о каталоге
            var dir = new DirectoryInfo(sourceDir);

            // Проверка на существование директории
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Собираем инфу о подпапках
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Создаем конечную папку (если её не существует)
            Directory.CreateDirectory(destinationDir);

            // Копируем все вложенные файлы в ориджин папке
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // Рекурсивно проходим по вложенным папкам
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        /// <summary>
        /// Копирование файла в новый файл с заданным именем
        /// </summary>
        /// <param name="sourceFilePath">Путь до родительского файла</param>
        /// <param name="destFilePath">Путь с указанием имени нового файла</param>
        static void CopyFile(string sourceFilePath, string destFilePath)
        {
            if (File.Exists(sourceFilePath))
            {
                try
                {
                    File.Copy(sourceFilePath, destFilePath, true); // параметр true разрешает перезапись файла, если он уже существует
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        /// <summary>
        /// Удаление каталога
        /// </summary>
        /// <param name="sourceDir">Путь к удаляемой папке</param>
        static void RemoveDir(string sourceDir)
        {
            if (Directory.Exists(sourceDir))
            {
                try
                {
                    Directory.Delete(sourceDir, true); // параметр true означает, что подпапки и файлы будут так же удалены рекурсивно
                }

                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

        /// <summary>
        /// Удаление файла
        /// </summary>
        /// <param name="sourceFilePath">Путь к файлу</param>
        static void RemoveFile(string sourceFilePath)
        {
            if (File.Exists(sourceFilePath))
            {
                try
                {
                    File.Delete(sourceFilePath);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }

        }
        
        /// <summary>
        /// Вывод всех доступных команд
        /// </summary>
        static void ShowHelp()
        {
            DrawWindow(0, 0, WINDOW_WIDTH, 26);
            string[] help = { "Список команд:",
                            "help - список всех команд",
                            "cd %path% - смена текущего каталога на введенный %путь%",
                            "ls %path% -p N - вывод дерева каталогов по указаному %пути% с указанием номера страницы",
                            "cp %source_path% %targer_path% - копирование каталога",
                            "cf %source_file_path% %target_file_path% - копирование файла",
                            "rm %path% - удаление каталога",
                            "rm %file_path% - удаление файла",
                            "file %file_path% - вывод инормации о файле",
                            "exit  - выход из приложения"};
            for (int i = 0; i < help.Length; i++)
            {
                Console.SetCursorPosition(1, 1+i);
                Console.WriteLine(help[i]);
            }
        }

        /// <summary>
        /// Отобразить дерево каталогов
        /// </summary>
        /// <param name="dir">Директория</param>
        /// <param name="page">Страница</param>
        static void DrawTree(DirectoryInfo dir, int page)
        {
            StringBuilder tree = new StringBuilder();
            GetTree(tree, dir, "", true);
            
            DrawWindow(0, 0, WINDOW_WIDTH, 26);
            (int currentLeft, int currentTop) = GetCursorPosition();
            int pageLines = 24;
            string[] lines = tree.ToString().Split('\n');
            int pageTotal = (lines.Length + pageLines-1) / pageLines;
            if (page>pageTotal)
               page = pageTotal;

            for (int i = (page - 1)*pageLines, counter = 0; i < page*pageLines; i++, counter++)
            {
                if (lines.Length - 1 > i)
                {
                    Console.SetCursorPosition(currentLeft + 1, currentTop + 1 + counter);
                    Console.WriteLine(lines[i]);
                }
            }

            //Отрисуем футер
            string footer = $"┤ {page} of {pageTotal} ├";
            Console.SetCursorPosition(WINDOW_WIDTH / 2 - footer.Length / 2, 25);
            Console.WriteLine(footer);
        }

        /// <summary>
        /// Получение в виде строки иерархии всех файлов и папок по заданном пути
        /// </summary>
        /// <param name="tree">Строка с деревом папок и файлов</param>
        /// <param name="dir">Директория</param>
        /// <param name="indent">Отступ</param>
        /// <param name="lastDirectory">Проверка на конечность директории</param>
        static void GetTree(StringBuilder tree, DirectoryInfo dir, string indent, bool lastDirectory)
        {
            tree.Append(indent);
            if (lastDirectory)
            {
                tree.Append("└─");
                indent += "  ";
            }
            else
            {
                tree.Append("├─");
                indent += "│ ";
            }

            tree.Append($"{dir.Name}\n");


            FileInfo[] subFiles = dir.GetFiles();
            for (int i = 0; i < subFiles.Length; i++)
            {
                if (i == subFiles.Length - 1)
                {
                    tree.Append($"{indent}└─{subFiles[i].Name}\n");
                }
                else
                {
                    tree.Append($"{indent}├─{subFiles[i].Name}\n");
                }
            }


            DirectoryInfo[] subDirects = dir.GetDirectories();
            for (int i = 0; i < subDirects.Length; i++)
                GetTree(tree, subDirects[i], indent, i == subDirects.Length - 1);
        }

        /// <summary>
        /// Сокращение пути директории
        /// </summary>
        /// <param name="path">Путь к директории</param>
        /// <returns></returns>
        static string GetShortPath(string path)
        {
            StringBuilder shortPathName = new StringBuilder((int)API.MAX_PATH);
            API.GetShortPathName(path, shortPathName, API.MAX_PATH);
            return shortPathName.ToString();
        }

        /// <summary>
        /// Обновление ввода с консоли
        /// </summary>
        static void UpdateConsole()
        {
            DrawConsole(GetShortPath(currentDir), 0, 36, WINDOW_WIDTH, 3);
            ProcessEnterCommand(WINDOW_WIDTH);
        }

        /// <summary>
        /// Отрисовка области консоли
        /// </summary>
        /// <param name="dir">Начальная директория</param>
        /// <param name="x">Позиция окна Х</param>
        /// <param name="y">Позиция окна У</param>
        /// <param name="width">Ширина окна</param>
        /// <param name="height">Высота окна</param>
        static void DrawConsole(string dir, int x, int y, int width, int height)
        {
            DrawWindow(x, y, width, height);
            Console.SetCursorPosition(x + 1, y + 1);
            Console.Write($"{dir}>");
        }

        /// <summary>
        /// Отрисовка окна
        /// </summary>
        /// <param name="x">Начальная позиция по Х</param>
        /// <param name="y">Начальная позиция по У</param>
        /// <param name="width">Ширина окна</param>
        /// <param name="height">Высота окна</param>
        static void DrawWindow(int x, int y, int width, int height)
        {
            //шапка
            Console.SetCursorPosition(x,y);
            Console.Write("┌");
            for(int i = 0; i < width-2; i++)
                Console.Write("─");
            Console.Write("┐");

            //стенки
            Console.SetCursorPosition(x, y + 1);
            
            for (int i = 0; i < height-2; i++)
            {
                Console.Write("│");
                for (int j = x + 1; j < x + width - 1; j++)
                    Console.Write(" ");
                Console.Write("│");
            }

            ///подвал
            Console.Write("└");
            for (int i = 0; i < width - 2; i++)
                Console.Write("─");
            Console.Write("┘");
            Console.SetCursorPosition(x, y);
        }
    }
}
