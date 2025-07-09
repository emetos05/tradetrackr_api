## General ASP.NET Core Instructions

- Utilize modern language features and C# versions whenever possible.
- Avoid outdated language constructs.
- Only catch exceptions that can be properly handled; avoid catching general exceptions. For example, sample code shouldn't catch the System.Exception type without an exception filter.
- Use specific exception types to provide meaningful error messages.
- Use LINQ queries and methods for collection manipulation to improve code readability.
- Use asynchronous programming with async and await for I/O-bound operations.
- Be cautious of deadlocks and use Task.ConfigureAwait when appropriate.
- Use the language keywords for data types instead of the runtime types. For example, use string instead of System.String, or int instead of System.Int32. This recommendation includes using the types nint and nuint.
- Use int rather than unsigned types. The use of int is common throughout C#, and it's easier to interact with other libraries when you use int. Exceptions are for documentation specific to unsigned data types.
- Use var only when a reader can infer the type from the expression. Readers view our samples on the docs platform. They don't have hover or tool tips that display the type of variables.
- Write code with clarity and simplicity in mind.
- Avoid overly complex and convoluted code logic.

- Use string interpolation to concatenate short strings, as shown in the following code.
- To append strings in loops, especially when you're working with large amounts of text, use a System.Text.StringBuilder object.
- Use the `using` statement to ensure proper disposal of resources, especially for types that implement IDisposable.
- Prefer raw string literals to escape sequences or verbatim strings.
- Use the expression-based string interpolation rather than positional string interpolation.

- Use Pascal case for primary constructor parameters on record types.
- Use camel case for primary constructor parameters on class and struct types.
- Use required properties instead of constructors to force initialization of property values.
- Use collection expressions to initialize all collection types.

- Use a try-catch statement for most exception handling.
- Simplify your code by using the C# using statement. If you have a try-finally statement in which the only code in the finally block is a call to the Dispose method, use a using statement instead.
- Always create an editable plan document to be used as part of context when working on a complex task.
- Complete tasks in simple testable chunks
