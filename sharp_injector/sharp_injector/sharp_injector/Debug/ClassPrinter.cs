using BetterAW;
using Newtonsoft.Json.Linq;
using sharp_injector.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace sharp_injector.Debug {
    public static class ClassPrinter {
        private static string addSpacing(string str, int len) {
            var numOfSpaces = len - str.Length;
            if (numOfSpaces > 0) {
                return str + ' ' * numOfSpaces;
            }
            return str + ' ';
        }


        public static void PrintMembers(string className) {
            var type = Type.GetType(className);
            var members = type.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var member in members) {
                printMeberInfo(member);
            }

        }

        public static void printMeberInfo(MemberInfo info) {

            switch (info.MemberType) {
                case MemberTypes.Property: {
                        var prop = info as PropertyInfo;
                        var toPrint = addSpacing($"Property: {prop.Name}", 60) + prop.PropertyType.FullName + '\n';
                        Terminal.Print(toPrint);
                    }
                    break;
                case MemberTypes.Method: {
                        var method = info as MethodInfo;
                        var toPrint = addSpacing($"Method: {method.Name}", 60) + method.ReturnType.FullName + '\n';
                        foreach (var arg in method.GetParameters()) {
                            toPrint += addSpacing($"   Var: {arg.Name}", 60) + arg.ParameterType.FullName + '\n';
                        }
                        Terminal.Print(toPrint);
                    }
                    break;
                case MemberTypes.Event: {
                        var envent = info as EventInfo;
                        var toPrint = addSpacing($"Event: {envent.Name}", 60) + envent.EventHandlerType.FullName + '\n';
                        Terminal.Print(toPrint);
                    }
                    break;
                case MemberTypes.Constructor: {
                        var constructor = info as ConstructorInfo;
                        var toPrint = addSpacing($"Constructor", 60) + $"{constructor.GetParameters().Length}\n";
                        foreach (var arg in constructor.GetParameters()) {
                            toPrint += addSpacing($"   Var: {arg.Name}", 60) + arg.ParameterType.FullName + '\n';
                        }
                        Terminal.Print(toPrint);
                    }
                    break;
                case MemberTypes.Field: {
                        var field = info as FieldInfo;
                        var toPrint = addSpacing($"Field: {field.Name}", 60) + field.FieldType.FullName + '\n';
                        Terminal.Print(toPrint);
                    }
                    break;
                case MemberTypes.TypeInfo: {
                        var type = info as TypeInfo;
                        var toPrint = $"TypeInfo: {type.FullName}\n";
                    }
                    break;
                default: {
                        var toPrint = $"Other member: {info.Name}\n";
                        Terminal.Print(toPrint);
                    }
                    break;
            }

        }
    }
}
