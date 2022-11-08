namespace TestsGeneratorDll.Tests
{
    public class LibraryUnitTests
    {
        public LibraryUnitTests()
        {
            List<string> files = new List<string>()
            {
                @"C:\Users\Gleb\Desktop\For Git\TestsGenerator\TestsGeneratorConsole\bin\Debug\TestsGeneratorConsole.exe",
                @"C:\Users\Gleb\Desktop\For Git\TestsGenerator\TestsGeneratorDll\bin\Debug\TestsGeneratorDll.dll"
            };
            TestsGenerator.GenerateXUnitTests(files, @"C:\Users\Gleb\Desktop\Results", 10);
        }

        [Fact]
        public void GenerateTests_WithSpecificFiles_ReturnRightNumberOfTestClassesGenerated()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\Users\Gleb\Desktop\Results");

            int filesCount = directoryInfo.GetFiles().Length;

            Assert.Equal(5, filesCount);
        }


        [Fact]
        public void GenerateTests_WithSpecificFiles_ReturnNotEmptyGeneratedFiles()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\Users\Gleb\Desktop\Results");

            var files = directoryInfo.GetFiles();

            bool isAnyFileEmpty = false;

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    isAnyFileEmpty = true;
                    break;
                }
            }

            Assert.False(isAnyFileEmpty);

        }
    }
}