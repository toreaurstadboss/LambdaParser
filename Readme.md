Lambda Parser
====

This is a lib for parsing code in runtime. The program language is C#.

The lib is forked manually from : https://github.com/zhucai/ which is a repo without license. 

Credentials to Zhucai for great work on the Lambda Parser.

I forked this lib to make it available on Nuget and Github and upgrade it to .NET Framework 4.8.
Plans are underway to use .NET Standard 2.1 instead. 


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

You can view test code to explore more functions:  https://github.com/zhucai/lambda-parser/blob/master/%20lambda-parser/Test_Zhucai.LambdaParser/ExpressionParserTest.cs