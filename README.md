# Современные Платформы Программирования (СПП). Lab №4

### Задача
Необходимо реализовать многопоточный генератор шаблонного кода тестовых классов для одной из библиотек для тестирования (`NUnit`, `xUnit`, `MSTest`) по тестируемым классам. (Мной был реализован вариант для `xUnit`).

### Затрагиваемые темы
1) `Многопоточное программирование` с использованием высокоуровневых библиотек.
2) `Асинхронное программирование` в .NET: `async/await`.
3) Модульное тестирование.
4) Мониторы Хоара.

### Входные данные

- Список файлов, для классов из которых необходимо сгенерировать тестовые классы (`.exe`, `.dll` и тп.).
- Путь к папке для записи созданных файлов.
- Ограничения на секции конвейера (см. далее).

### Выходные данные

- Файлы с тестовыми классами (по одному тестовому классу на файл, вне зависимости от того, как были расположены тестируемые классы в исходных файлах) (`p.s.` В этом пункте мной было принято решение создавать по однопу тестовому файлу на класс, то есть для каждого класса будет построен отдельный файл с классом, в котором будут находиться `заглушки` проверки методов этого класса. Такой подход не только поможет избежать проблем с повторяющимися названиями методов (т.к. методы будут привязаны к разным классам, то проблемы возникнуть не может), но и поможет создать полноценные тестовые классы с разделяемыми предметными областями).
- Все сгенерированные тестовые классы должны компилироваться при включении в отдельный проект, в котором имеется ссылка на проект с тестируемыми классами.
- Все сгенерированные тесты должны завершаться с ошибкой (или же каждый тест должен включать в себя `Assert.Fail`).

### Схема работы

Генерация выполняется в конвейерном режиме "производитель-потребитель" (в качестве потребителя выступает консольная программа, а в качестве производителя - `.dll` библиотека, включающая в себя сервисы: `AssembliesProvider`, `MethodsProvider`, `XUnitTestsProvider` и `FileStreamProvider`, которые работают в том же самом режиме "производитель-потребитель") и состоит из трех основных этапов: 

- Параллельная загрузка исходных текстов в память (с ограничением количества файлов, загружаемых за раз);
- Генерация тестовых классов в многопоточном режиме (с ограничением максимального количества одновременно обрабатываемых задач); 
- Параллельная запись результатов на диск (с ограничением количества одновременно записываемых файлов).

`p.s.` Мной также был добавлен четвёртый пункт между `1` и `2` :

  - Получение методов для генерации для них тестовых классов из загруженных исходных текстов
  
  **При реализации использовались такие технологии, как:**
  
  - `Async/await` и асинхронный `API`
  - Мониторы хоара (класс `Monitor`)
  - `ThreadPool` и взаимодействие с системным пулом потоков
  
  **Пример** исходного класса для которого необходимо было сгенерировать тесты для `xUnit`:
  
```C#
public class MyClass
    {
        public void FirstMethod()
        {
            Console.WriteLine("First method");
        }

        public void SecondMethod()
        {
            Console.WriteLine("Second method");
        }

        public void ThirdMethod(int a)
        {
            Console.WriteLine("Third method (int)");
        }

        public void ThirdMethod(double a)
        {
            Console.WriteLine("Third method (double)");
        }
    }
```

  **Результат** программы и сгенерированные ею тесты:
  
  ```C#
public class MyClassTESTS
{

	[Fact]
	public void FirstMethod_WithoutParameters_ReturnAssertFailure
	{
		Assert.Fail("autogenerated");
	}

	[Fact]
	public void SecondMethod_WithoutParameters_ReturnAssertFailure
	{
		Assert.Fail("autogenerated");
	}

	[Fact]
	public void ThirdMethod_WithInt32Parameter_ReturnAssertFailure
	{
		Assert.Fail("autogenerated");
	}

	[Fact]
	public void ThirdMethod_WithDoubleParameter_ReturnAssertFailure
	{
		Assert.Fail("autogenerated");
	}

}
```


### Организация кода

Код лабораторной работы состоит из трех проектов:

- `Библиотека` для генерации тестовых классов, содержащая логику по разбору исходного кода и многопоточной генерации классов, а также загрузке исходных текстов в память и сохранение результатов работы в файлы.
- `Модульные тесты` для главной библиотеки.
- `Консольная программа`, демонстрирующая функциональные возможности библиотеки.


## Техническая часть

Для данной лабораторной работы, мной было принято решение попробовать совместить сразу несколько подходов многопоточного программирования, а также опробовать новую для себя технологию работы в многопоточном режиме и синхронизации происходимых в потоках действий над одной переменной при помощи использования мьютексов (объектов синхронизации), а также использования класса `Monitor` языка программирования C#.

### Работа библиотеки для генерации тестов

Разработанная мной библиотека имеет только один доступный для использования программистом класс, который содержит в себе единственный статический метод для генерации тестовых кассов и сохранения их в файлы:

```C#
        public static void GenerateXUnitTests(List<string> filePaths, string savingPath, int restriction)
```

где:

- `filePath` - Пути к файлам, для которых нужно сгенерировать тестовые классы
- `savingPath` - Путь, по которому будут сохранены тестовые классы
- `restriction` - Ограничение на максимальное количество одновременно обрабатываемых задач

### Касательно сервисов, которыми пользуется моя библиотека

Всего было разработано 4 сервиса:

#### AssembliesProvider или сервис для предоставления исходных текстов переданных программ

Данный сервис получает на вход список файлов, для которых необходимо загрузить исходные текста, и предоставить их для дальнейшей обработки. 

#### MethodsProvider или сервис для извлечения списка методов из переданных сборок

Данный сервис получает на вход N-ное количество сборок `Assemblies`, для которых генерирует список объектов `MethodInfo` методов, для которых необходимо сгенерировать тесты, после чего передаёт их дальше на обработку.

#### XUnitTestsProvider или сервис для генерации текстового представления тестовых методов

Данный сервис получает на вход список методов, для которых нужно сгенерировать тесты, после чего обрабатывает их, и на выходе предоставляет список строкового представления тестов для их дальнейшей записи в файлы.

#### FileStreamProvider или сервис для сохранения текстового представления тестов в файлы

Данный сервис получает на вход список строковых переменных тестов, которые необходимо сохранить в файлы по переданнопу пути, после чего работа сервиса завершается, а задача генерации тестов считается завершённой.

### Вывод

Таким образом, мной было разработано многопоточное приложение, способное генерировать тестовые классы для библиотеки `xUnit` из набора произвольных программ, написанных на языке программирования C#.


