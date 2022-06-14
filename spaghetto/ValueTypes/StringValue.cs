﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spaghetto {
    public class StringValue : Value {
        public static Class ClassImpl = new("String", new()
        {
            {"toNumber",
                new NativeFunction("toNumber", (List<Value> args, Position posStart, Position posEnd, Context ctx) => {
                    bool success = double.TryParse(args[0].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result);
                    if (!success) throw new RuntimeError(posStart, posEnd, "Argument 'self' is not a valid number", ctx);
                    return new Number(result);
                }, new() { "self" }, false)
            },
        }, new()
        {
            { "empty", new StringValue("") },
        });

        new public string value;

        public StringValue(string str) {
            this.value = str;
        }

        public override Value Copy() {
            return new StringValue(value).SetPosition(posStart, posEnd).SetContext(context);
        }

        public override (Value, SpaghettoException) AddedTo(Value other) {
            if (other is StringValue) {
                return (new StringValue(value + (other as StringValue).value).SetContext(context), null);
            }

            return (null, new TypeError(posStart, posEnd, "Can not perform AddedTo with string to " + other.GetType().Name));
        }

        public override (Value, SpaghettoException) MultipliedBy(Value other) {
            if (other is Number) {
                return (new StringValue(value.Repeat((int)(other as Number).value)).SetContext(context), null);
            }

            return (null, new TypeError(posStart, posEnd, "Can not perform MultiplyBy with string to " + other.GetType().Name));
        }

        public override (Value, SpaghettoException) IsEqualTo(Value other) {
            if (other is StringValue) {
                return (new Number(value == (other as StringValue).value ? 1 : 0), null);
            }

            return (null, new TypeError(posStart, posEnd, "Can not perform IsEqualTo with string to " + other.GetType().Name));
        }

        public override (Value, SpaghettoException) IsNotEqualTo(Value other) {
            if (other is StringValue) {
                return (new Number(value == (other as StringValue).value ? 0 : 1), null);
            }

            return (null, new TypeError(posStart, posEnd, "Can not perform IsEqualTo with string to " + other.GetType().Name));
        }

        public override string Represent() {
            return $"\"{value}\"";
        }

        public override bool IsTrue() {
            return (value.Length > 0);
        }

        public override string ToString() {
            return value;
        }

        public override Value Get(string identifier)
        {
            return ClassImpl.Get(identifier);
        }
    }
}
