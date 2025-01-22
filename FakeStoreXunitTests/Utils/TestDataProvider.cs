using Newtonsoft.Json;

namespace FakeStoreXunitTests.Utils;

public static class TestDataProvider
{
    public static readonly string TestDirectory = Directory
        .GetParent(Directory.GetCurrentDirectory())
        ?.Parent?.Parent?.FullName!;

    public static IEnumerable<object[]> GetTestData<T>(string filePath)
    {
        var json = File.ReadAllText(Path.Combine(TestDirectory, filePath));
        var data = JsonConvert.DeserializeObject<List<T>>(json)!;

        foreach (var item in data)
        {
            yield return new object[] { item! };
        }
    }
}
