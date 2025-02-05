/*
This library is just a basic example of
something your webpage may need to call.

Place it in the 'references' folder and
it will become available to both the 
runner and compiler automatically
*/

namespace ExampleLibrary;
public class MyCoolThing(int number)
{
  private readonly int BestNumber = number;
  public string AmazingMethod => $"Hello World! [{BestNumber}]";
}
