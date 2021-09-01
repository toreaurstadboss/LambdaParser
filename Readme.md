## Lambda Parser

Generic lambda parser for C# which parses strings containing lambda expressions into 
code (delegates and funcs) at runtime, which can then be invoked to either run or return values.

### Last version : 1.1.0
### Last updated: 01.09.2021

This is a lib for parsing code in runtime. The program language is C#.

The lib supports lambda expressions and converts them to runnable code. Passing in a lambda expression
creates a delegate or func and makes it possible to invoke it and get the results. 

_See the unit tests for examples of supported formats_. 

If you have examples about lambda expressions which should be supported, feel free to drop a Pull Request or
report an issue in this repo, and I will try to look into it.

Of course, EVERY variant of a lambda expression is hard to support, but this library 
do support many different lambda expressions. In case you find a bug, feel free to report an issue.

The lib is forked manually from : https://github.com/zhucai/ which is a repo without license. I have added the MIT License now.
You are FREE to use this library on your own responsibility! 

Credentials to **Zhucai** for great work on the Lambda Parser! 

- I have quality assured the lib. I have found a few bugs which is now fixed. 
I forked this lib to make it available on Nuget and Github and upgrade it from .NET Framework 3.5 to .NET Framework 4.8.
Plans are underway to use .NET Standard 2.1 instead. 
- The source code has now been translated from Chinese to English including exception messages and NDOC code comments.

The following issues has been fixed: 
* Now possible to use enum values inside lambda expressions strings.
* Bug in parsing double values and considering culture dependent number decimal separators is now fixed.
  In case you get a value '3.14' and your culture expects numbers written with comma as number decimal separator '3,14' the parsing will now suceed.

##### Notes about performance
The library used about 1-5 ms in average on each unit test, some lambdas take more time such as delegates with arrays and lists. 
Please note that using complex lambdas in strings you want to compile to expressions will take more time, the more detailed the 
lambda expression is. Therefore, this library should be used with relatively simple or mundane lambda expresions for performance considerations.
Or just consider caching the expression tree(s) into memory would be another options.


#### Example1 - string concat and number calculate:
```C#
string code = "2.ToString()+(4*2)"; // C# code
Func<string> func = ExpressionParser.Compile<Func<string>>(code); // compile code
string result = func(); // result = "28"
```

#### Example2 - input parameter:
```C#
Delegate dele = ExpressionParser.Compile("(int m)=>-m");
var result = (int)dele.DynamicInvoke(10); // result = -10
```

#### Example3 - access property in anonymous type:
```C#
//using Zhucai.LambdaParser.ObjectDynamicExtension;
object obj = new { Name = "zhangsan", Id = 18 }; // maybe get [obj] from method return value
int result = obj.E<int>("Id"); // result = 18

```

#### Example 4 - passing in a model and calculate a compound boolean expression. 
The lib now supports enum values.

```C#
        [Test]
        public void ParseDelegateTest_OperatorWithModelInstanceEqual()
        {
            string code = "m => m.SomeEnumValue == SomeEnumDataContract.Ja && m.SomeBoolean && m.SomeDateTime > new DateTime(2021, 1, 1)";
            bool expected = true;

            var someSkjema = new SomeFormDataContract
            {
                FormGuid = Guid.NewGuid(),
                SomePatientGuid = Guid.NewGuid(),
                SomeEnumValue = SomeEnumDataContract.Ja,
                SomeBoolean = true,
                SomeDateTime = new DateTime(2021, 1, 2)
            };
            Func<SomeFormDataContract, bool> func = ExpressionParser.Compile<Func<SomeFormDataContract, bool>>(code, "System", "Common");
            bool actual = func(someSkjema);
            Assert.AreEqual(expected, actual);
        }
```

You can view test code to explore more functions, there are several more lambda expressions which are supported:
https://github.com/toreaurstadboss/LambdaParser/blob/main/LambdaParser/Test_Zhucai.LambdaParser/ExpressionParserTest.cs


### Building new versions of this lib

Quick Note ! Remember to switch to Release before building the entire solution in Visual Studio and to avoid pushing a nuget with Debug to production as this will run slower ! 

Just edit the .nuspec file in Solution items and run **nuget pack** inside the **ToreAurstadIT.LambdaParser** project.
Feel free to fork the lib and make it your own. The goal here is to help developers in the C# community in achieving their tasks 
in parsing a lambda expression written as a string into runnable code. 

```bash
cd ToreAurstadIT.LambdaParser
nuget pack
```

Again, great work of zhucai in doing most of the grunt work here, I just merely polished the lib fixing some few issues.

Last update 01.092.2021