## Lambda Parser

This is a lib for parsing code in runtime. The program language is C#.

The lib supports lambda expressions and converts them to runnable code. Passing in a lambda expression
creates a delegate or func and makes it possible to invoke it and get the results. 

See the unit tests for examples of supported formats. Of course, EVERY variant of a lambda expression is hard to support, but this library 
do support many different lambda expressions. In case you find a bug, feel free to report an issue.

The lib is forked manually from : https://github.com/zhucai/ which is a repo without license. I have added the MIT License now.
You are FREE to use this library on your own responsibility! 

Credentials to **Zhucai** for great work on the Lambda Parser! 

- I have just polished and quality assured the lib a little bit. I have found a few bugs which is now fixed. 

I forked this lib to make it available on Nuget and Github and upgrade it to .NET Framework 4.8.
Plans are underway to use .NET Standard 2.1 instead. 

The following issues has been fixed: 
* Now possible to use enum values inside lambda expressions strings.
* Bug in parsing double values and considering culture dependent number decimal separators is now fixed.
  In case you get a value '3.14' and your culture expects numbers written with comma as number decimal separator '3,14' the parsing will now suceed.

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

You can view test code to explore more functions:  https://github.com/toreaurstadboss/LambdaParser/blob/main/LambdaParser/Test_Zhucai.LambdaParser/ExpressionParserTest.cs

Last update 01.092.2021