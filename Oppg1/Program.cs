
        
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace Oppg1;

public enum Operations{Multiply = 0, Divide = 1, Add = 2, Subtract = 3, Null =-1}



class Program 
{
    static void Main(string[] args)
    {
        string? input;
        List<double> numbers = [];
        List<Operations> operators = [];
        bool done = false; bool numberAccepted = false;
        //Set up an empty Operator Enum
        Operations operation = Operations.Add;
        //Set up a new calculator
        Calculator calculator = new();
        //Set up three nullable strings
        double a;
        //Operations? op1;
        //Ask user for input
        Console.WriteLine("First number?");
        input = Console.ReadLine();
        if(input == null) throw new WarningException();
        a = calculator.ParseNumber(input);
        
        numbers.Add(a);

        while (!done)
        {
            done = false; numberAccepted = false;
            Console.WriteLine("Your operator? (Add, Sub, Mult or Div)");
            Console.WriteLine("Return blank response or type end/done/calc to calculate.");
            input = Console.ReadLine();
            if(input == null) throw new WarningException();

            if(calculator.CheckForEscapeCharacter(input)) {done = true; break;}
            else {operation = calculator.SetOperator(input); operators.Add(operation);}
            
            while (!numberAccepted)
            {
                Console.WriteLine("Next number?");
                input = Console.ReadLine();
                if(input == null) throw new WarningException();

                a = calculator.ParseNumber(input);
                numberAccepted = true; 
                numbers.Add(a);
            }
        }

        Console.WriteLine("Sum is: " + calculator.Calculate(numbers, operators));


    }

    
}

class Calculator : ICalculator
{
    public double Calculate(List<double> numbers, List<Operations> operators)
    {
        Console.WriteLine("Setting up variables for calculation");
        double tempResult=0;
        double result = 0;
        List<double> orderedNumbers = [];
        List<Operations> orderedOperations = operators;
        List<MathEquation> mathEquations = [];
        List<MathEquation> mathResults = [];
        List<double> results = [];

        if(operators.Count > 1){
            for (int i = 0; i < operators.Count; i+=2)
            {
                //Every loop should result in one math problem and an associated score
                Console.WriteLine($"Loop {i} out of {operators.Count}");
                if(i==0){ //First equation, this should not have a prefacing operator, or operator should be add?
                    MathEquation mathEquation = CreateMathEquation(numbers[i], numbers[i+1],operators[i], Operations.Null, i);
                    Console.WriteLine($"{numbers[i]} {operators[i]} {numbers[i+1]}");
                    mathEquations.Add(mathEquation);
                }
                else { //Paired equations, gets operator from i-1 (Previous operator)
                    MathEquation mathEquation = CreateMathEquation(numbers[i], numbers[i+1],operators[i], operators[i-1], i);
                    Console.WriteLine($"{numbers[i]} {operators[i]} {numbers[i+1]}");
                    mathEquations.Add(mathEquation);
                }
                
                //Check operators[i+1]. How should it work?
                //10+10+10+10+10
                //-> 10+10  +10+10  +10
                //i = 0 will target the first *, then i+1 will take the second *
                //The next mathequation will start from the third *
            }
            
        } else { 
            MathEquation mathEquation = CreateMathEquation(numbers[0], numbers[1],operators[0], operators[0], operators.Count);
            mathEquations.Add(mathEquation);
        }

        //Order the list: Mult and divide operators first, depending on their location in the formula.
        //Addition, then Subtraction
        Console.WriteLine("Ordering list of equations");
        mathEquations = mathEquations.OrderBy(x => x.PEMDAS_Value).ThenByDescending(x => x.locationValue).ToList();

        //Go through every new ordered math equation, do their calculation.
        //Log the result to a new result list, then add all results together. 
        for (int i = 0; i < mathEquations.Count; i++)
        {
            Console.WriteLine("Step solving equations");
            Console.WriteLine($"Equation: {mathEquations[i].a} {mathEquations[i].operation} {mathEquations[i].b}");
            if(mathEquations[i].b == 0){ //This should mean that this equation is the last one
            
                switch (mathEquations[i].operation)
                {
                    case Operations.Add:
                        mathResults.Add(CreateMathEquation(mathEquations[i].a,0,mathEquations[i].operation,Operations.Null,mathEquations[i].locationValue));
                        break;
                    case Operations.Subtract:
                        mathResults.Add(CreateMathEquation(mathEquations[i].a,0,mathEquations[i].operation,Operations.Null,mathEquations[i].locationValue));                
                        break;
                    case Operations.Multiply:
                        mathResults.Add(CreateMathEquation(mathEquations[i].a,0,mathEquations[i].operation,Operations.Null,mathEquations[i].locationValue));                
                        break;
                    case Operations.Divide:
                        mathResults.Add(CreateMathEquation(mathEquations[i].a,0,mathEquations[i].operation,Operations.Null,mathEquations[i].locationValue));
                        break;
                    default:
                        break;      
                }
                    
            }
            else
            {
                switch (mathEquations[i].operation)
                {
                    case Operations.Add:
                        tempResult = Add(mathEquations[i].a,mathEquations[i].b);
                        mathResults.Add(CreateMathEquation(tempResult,0,mathEquations[i].nextOp,Operations.Null,mathEquations[i].locationValue));
                        break;
                    case Operations.Subtract:
                        tempResult = Subtract(mathEquations[i].a,mathEquations[i].b);
                        mathResults.Add(CreateMathEquation(tempResult,0,mathEquations[i].nextOp,Operations.Null,mathEquations[i].locationValue));
                        break;
                    case Operations.Multiply:
                        tempResult = Multiply(mathEquations[i].a,mathEquations[i].b);
                        mathResults.Add(CreateMathEquation(tempResult,0,mathEquations[i].nextOp,Operations.Null,mathEquations[i].locationValue));
                        break;
                    case Operations.Divide:
                        tempResult = Divide(mathEquations[i].a,mathEquations[i].b);
                        mathResults.Add(CreateMathEquation(tempResult,0,mathEquations[i].nextOp,Operations.Null,mathEquations[i].locationValue));
                        break;
                    default:
                        break;      
                }
            }
        }
        
        Console.WriteLine("Result count: "+mathResults.Count);

        if(mathResults.Count > 1){
            for (int i = 0; i < mathResults.Count; i+=2)
            {
            if(i+1>mathResults.Count) break;
            switch (mathResults[i].operation)
            {
            case Operations.Add:
                result += Add(mathResults[i].a, mathResults[i+1].a);
                Console.WriteLine($"{mathResults[i].a} * {mathResults[i+1].a}");
                break;
            case Operations.Subtract:
                result += Subtract(mathResults[i].a, mathResults[i+1].a);
                Console.WriteLine($"{mathResults[i].a} * {mathResults[i+1].a}");

                break;
            case Operations.Multiply:
                result += Multiply(mathResults[i].a, mathResults[i+1].a);
                Console.WriteLine($"{mathResults[i].a} * {mathResults[i+1].a}");
                break;
            case Operations.Divide:
                result += Divide(mathResults[i].a, mathResults[i+1].a);
                Console.WriteLine($"{mathResults[i].a} * {mathResults[i+1].a}");
                break;
            default:
                break;      
            }

            //Below line is not correct. Find a way to get the unused operator from the equation, then
            //Switch through the operation to know the correct operator to use
            //tempResult += Add(results[i], results[i+1]);
            //Console.WriteLine($"{results[i]} + {results[i+1]} = {tempResult}");          
            }
        }else return mathResults[0].a;
        
        
        return result;
    }

    public double Add(double a, double b)
    {
        return a + b;
    }

    public double Subtract(double a, double b)
    {
        return a - b;
    }

    public double Multiply(double a, double b)
    {
        return a * b;
    }

    public double Divide(double a, double b)
    {
        if(b == 0 || b.Equals(0)) 
        {
            Console.WriteLine("Cannot divide by 0");
        }
        return a / b;
    }

    public MathEquation CreateMathEquation(double a, double b, Operations op, Operations prefacingOpr, int locationValue)
    {
        MathEquation mathEquation = new();
        mathEquation.a = a;
        mathEquation.b = b;
        mathEquation.operation = op;
        mathEquation.locationValue = locationValue;
        switch (op)
            {
                case Operations.Add:
                    mathEquation.PEMDAS_Value = 0;         
                    break;
                case Operations.Subtract:
                    mathEquation.PEMDAS_Value = 1;
                    break;
                case Operations.Multiply:
                    mathEquation.PEMDAS_Value = 2;
                    break;
                case Operations.Divide:
                    mathEquation.PEMDAS_Value = 2;
                    break;
                default:
                    break;
            }
        return mathEquation;
    }

    public int ParseNumber(string number){
        if(!int.TryParse(number, out int parsedNumber))
        {
            
            Console.WriteLine("All numbers must be a valid value. Doubles with numbers only.");
            throw new WarningException();
        }
        else return parsedNumber;
        
    }

    public bool CheckForEscapeCharacter(string strToCheck){
            if(String.Equals(strToCheck, "Done", StringComparison.OrdinalIgnoreCase)
            || String.Equals(strToCheck, "End", StringComparison.OrdinalIgnoreCase)
            || String.Equals(strToCheck, "Calc", StringComparison.OrdinalIgnoreCase)
            || String.Equals(strToCheck, "", StringComparison.OrdinalIgnoreCase)) return true;
            else return false;
    }

    public Operations SetOperator(string op){
        //Is it better to get the reference to operation from Main instead of creating a new one every time?
        Operations operation = new();
        //We do a Ignore Case stringcomparison to see if user has entered any valid input
        if(String.Equals(op, "Add", StringComparison.OrdinalIgnoreCase)
        || String.Equals(op, "Addition", StringComparison.OrdinalIgnoreCase)
        || op == "+") operation = Operations.Add;
        
        //Repeated for every keyword
        else if(String.Equals(op, "Sub", StringComparison.OrdinalIgnoreCase)
        || String.Equals(op, "Subtraction", StringComparison.OrdinalIgnoreCase)
        || op == "-") operation = Operations.Subtract;

        //If found, we set the operator accordingly
        else if(String.Equals(op, "Mult", StringComparison.OrdinalIgnoreCase)
        || String.Equals(op, "Multiply", StringComparison.OrdinalIgnoreCase)
        || op == "*") operation = Operations.Multiply;
      
        else if(String.Equals(op, "Div", StringComparison.OrdinalIgnoreCase)
        || String.Equals(op, "Divide", StringComparison.OrdinalIgnoreCase)
        || op == "/") operation = Operations.Divide;
        
        //If no valid operators are found, we throw a warning exception
        else throw new WarningException();
        //We return the newly set operation.
        return operation;
    }
}

interface ICalculator{
    
    /// <summary>
    /// Add two number together
    /// </summary>
    /// <param name="a">the value of the number a</param>
    /// <param name="b">the value of the number b</param>
    /// <returns>a + b</returns>
    double Add(double a, double b);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a">the value of the number a</param>
    /// <param name="b">the value of the number b</param>
    /// <returns>a - b</returns>
    double Subtract(double a, double b);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a">the value of the number a</param>
    /// <param name="b">the value of the number b</param>
    /// <returns>a * b</returns>
    double Multiply(double a, double b);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a">the value of the number a</param>
    /// <param name="b">the value of the number b</param>
    /// <returns>a / b</returns>
    double Divide(double a, double b);

    double Calculate(List<double> numbers, List<Operations> operators);

    int ParseNumber(string opToParse);

    Operations SetOperator(string op);

    bool CheckForEscapeCharacter(string strToCheck);

    MathEquation CreateMathEquation(double a, double b, Operations op, Operations prefacingOpr, int locationValue);
}

public class MathEquation(){
    public int PEMDAS_Value;
    public int locationValue;
    public double a,b;
    public Operations operation;
    public Operations nextOp;
}



