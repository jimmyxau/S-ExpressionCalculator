using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SExpressionCalculatorV2
{
    class Program
    {
        private static readonly string[] operations = { "add", "multiply" };

        private enum CalculationFunction
        {
            unknown,
            add,
            multiply
        }

        private static CalculationFunction ConvertFunctionToEnum(string function)
        {
            function = function.ToLower();

            switch (function)
            {
                case "add":
                    return CalculationFunction.add;
                case "multiply":
                    return CalculationFunction.multiply;
                default:
                    return CalculationFunction.unknown;
            }
        }

        private static int CalculateOperationFunction(CalculationFunction calculationFunction, List<int> numbers)
        {
            int total = 0;

            switch (calculationFunction)
            {
                case CalculationFunction.add:
                    foreach (var number in numbers)
                    {
                        total += number;
                    }
                    return total;

                case CalculationFunction.multiply:
                    total = 1;
                    foreach (var number in numbers)
                    {
                        total = total * number;
                    }
                    return total;

                default:
                    return -1;
            }
        }

        private static bool IsOperation(string input)
        {
            input = input.ToLower();

            if (input.IndexOf(CalculationFunction.add.ToString()) > -1 ||
                input.IndexOf(CalculationFunction.multiply.ToString()) > -1)
            {
                return true;
            }

            return false;
        }

        private static void AddItemInList(List<int> list, string value)
        {
            value = value.Trim().Replace(")", "");

            //if(Regex.IsMatch(value, "^[0-9]*$"))
            if (int.TryParse(value, out int num))
            {
                list.Add(num);
            }
            else
            {
                ShowErrorMsg();
            }
        }

        private static void CalculateIndividualExpression(string value, out int total, List<int> curOpeDigits = null)
        {
            var values = value.Trim().Replace(")", "").Split(' ');

            var operation = ConvertFunctionToEnum(values[0]);

            if (curOpeDigits == null)
            {
                curOpeDigits = new List<int>();
            }

            for (int i = 1; i < values.Length; i++)
            {
                curOpeDigits.Add(Convert.ToInt32(values[i]));
            }

            total = CalculateOperationFunction(operation, curOpeDigits);
        }

        private static void ShowErrorMsg()
        {
            Console.WriteLine("Syntax Error.");
        }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                //Console.WriteLine("Please enter an expression argument.");
                //Console.WriteLine("Usage: Program (add 1 1) or Program (multiply 1 1)");
                return 1;
            }

            var inputStr = "";

            for (int i = 0; i < args.Length; i++)
            {
                if (!string.IsNullOrEmpty(inputStr))
                    inputStr += " ";

                inputStr += args[i];
            }

            //Console.WriteLine("input string: " + inputStr);

            var total = 0;

            var pattern = @"(\([A-Za-z]* ([0-9] *)+\))";

            var url = "C:\\temp\\output.bat";

            if (Int32.TryParse(inputStr, out int value))
            {
                total = value;
            }
            else
            {
                var inputClone = inputStr;

                while (inputClone.IndexOf('(') >= 0)
                {
                    var matchReg = Regex.Match(inputClone, pattern);

                    var matchRegVal = matchReg.Value;

                    var currentIndex = inputClone.IndexOf(matchRegVal);

                    var currentFuncList = matchRegVal.Substring(1, matchRegVal.Length - 2).Split(' ');

                    var currentOperation = ConvertFunctionToEnum(currentFuncList[0]);

                    var numbers = new List<int>();

                    for (int i = 1; i < currentFuncList.Length; i++)
                    {
                        numbers.Add(Convert.ToInt32(currentFuncList[i]));
                    }

                    total = CalculateOperationFunction(currentOperation, numbers);

                    inputClone = inputClone.Replace(matchRegVal, total.ToString());
                }                             
            }

            File.WriteAllText(url, $"@echo off \necho {total}");

            ProcessStartInfo startInfo = new ProcessStartInfo($"{url}")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            //var process = new Process
            //{
            //    StartInfo = new ProcessStartInfo
            //    {
            //        //FileName = "C:\\Windows\\System32\\fsutil.exe",
            //        FileName = $"{url}",
            //        //Arguments = "behavior query SymlinkEvaluation",
            //        //Arguments = "echo",

            //        UseShellExecute = false,
            //        RedirectStandardOutput = true,
            //        CreateNoWindow = true
            //    }
            //};

            var process = new Process()
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += CaptureOutput;
            process.ErrorDataReceived += CaptureError;


            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            //process.StandardInput.WriteLine($"");

            //while (!process.StandardOutput.EndOfStream)
            //{
            //    var line = process.StandardOutput.ReadLine();
            //    Console.WriteLine(line);
            //}

            process.WaitForExit();

            //if (File.Exists(url))
            //{
            //    File.Delete(url);
            //}

            /*
            try
            {
                var functions = inputStr.Split(new char[] { '(' });
                var total = 0;
                var tempVal = 0;

                if (functions.Length == 1)
                {
                    if (int.TryParse(functions[0], out int num))
                        total = num;
                }
                else if (functions.Length == 2)
                {
                    CalculateIndividualExpression(functions[1], out total);
                }
                else
                {
                    List<int> avaiDigits = new List<int>();

                    for (int k = functions.Length - 1; k >= 0; k--)
                    {
                        var func = functions[k];

                        if (!string.IsNullOrEmpty(func))
                        {
                            if (func.IndexOf(')') > 0 && func.IndexOf(')') + 1 < func.Length - 1)
                            {
                                var expressions = func.Trim().Split(')');

                                foreach (var exp in expressions)
                                {
                                    if (!string.IsNullOrEmpty(exp))
                                    {
                                        if (!IsOperation(exp))
                                        {
                                            AddItemInList(avaiDigits, exp);
                                        }
                                        else
                                        {
                                            CalculateIndividualExpression(exp, out tempVal);
                                            avaiDigits.Add(tempVal);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!IsOperation(func))
                                {
                                    AddItemInList(avaiDigits, func);
                                }
                                else if (func.Contains(')') && IsOperation(func))
                                {
                                    CalculateIndividualExpression(func, out tempVal);
                                    avaiDigits.Add(tempVal);
                                }
                                else if (IsOperation(func))
                                {
                                    CalculateIndividualExpression(func, out total, avaiDigits);

                                    avaiDigits = new List<int>
                                        {
                                            total
                                        };
                                }
                            }
                        }
                    }
                    avaiDigits = null;
                }

                Console.WriteLine("Result: " + total);
            }
            catch (Exception ex)
            {
                ShowErrorMsg();
                var msg = ex.Message;
            }
            */
            return total;
        }

        static void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data, ConsoleColor.Green);
        }

        static void CaptureError(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data, ConsoleColor.Red);
        }

        static void ShowOutput(string data, ConsoleColor color)
        {
            if (data != null)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine("Output: {0}", data);
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
