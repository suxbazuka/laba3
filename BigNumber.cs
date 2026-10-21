using System;
using System.Text;

namespace Clicker
{
    /// <summary>
    /// Неизменяемое неотрицательное целое число произвольной длины.
    /// Хранится как массив блоков по 4 десятичных разряда (little-endian:
    /// _blocks[0] - младший блок). Все операции реализованы поразрядно,
    /// без long / double / System.Numerics.BigInteger.
    /// </summary>
    public sealed class BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>
    {
        private const int Base = 10000;      // 10^4: произведение двух блоков (< 10^8) помещается в int
        private const int BlockLength = 4;   // число десятичных цифр в блоке

        private readonly int[] _blocks;      // readonly + private => объект нельзя изменить после создания

        public static readonly BigNumber Zero = new BigNumber(new int[] { 0 });
        public static readonly BigNumber One = new BigNumber(1);

        // ---------- Конструкторы ----------

        private BigNumber(int[] blocks)
        {
            _blocks = Trim(blocks);
        }

        public BigNumber(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "BigNumber не может быть отрицательным.");

            if (value == 0)
            {
                _blocks = new int[] { 0 };
                return;
            }

            int count = 0;
            for (int t = value; t > 0; t /= Base) count++;

            int[] blocks = new int[count];
            for (int i = 0; i < count; i++)
            {
                blocks[i] = value % Base;
                value /= Base;
            }
            _blocks = blocks;
        }

        public static BigNumber Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new FormatException("Пустая строка.");

            s = s.Trim();
            foreach (char c in s)
            {
                if (c < '0' || c > '9')
                    throw new FormatException("Допустимы только цифры 0-9.");
            }

            int count = (s.Length + BlockLength - 1) / BlockLength;
            int[] blocks = new int[count];

            for (int i = 0; i < count; i++)
            {
                int end = s.Length - i * BlockLength;
                int start = Math.Max(0, end - BlockLength);
                int value = 0;
                for (int k = start; k < end; k++)
                    value = value * 10 + (s[k] - '0');
                blocks[i] = value;
            }

            return new BigNumber(blocks);
        }

        // ---------- Свойства ----------

        public bool IsZero
        {
            get { return _blocks.Length == 1 && _blocks[0] == 0; }
        }

        // ---------- Приватная арифметика (вся логика здесь) ----------

        // Убираем ведущие нулевые блоки (кроме единственного нуля).
        private static int[] Trim(int[] blocks)
        {
            if (blocks == null || blocks.Length == 0)
                return new int[] { 0 };

            int len = blocks.Length;
            while (len > 1 && blocks[len - 1] == 0) len--;

            if (len == blocks.Length) return blocks;

            int[] result = new int[len];
            Array.Copy(blocks, result, len);
            return result;
        }

        // Сложение столбиком: блок за блоком с переносом.
        private static BigNumber Add(BigNumber a, BigNumber b)
        {
            int len = Math.Max(a._blocks.Length, b._blocks.Length);
            int[] result = new int[len + 1];
            int carry = 0;

            for (int i = 0; i < len; i++)
            {
                int sum = carry;
                if (i < a._blocks.Length) sum += a._blocks[i];
                if (i < b._blocks.Length) sum += b._blocks[i];

                result[i] = sum % Base;
                carry = sum / Base;
            }
            result[len] = carry;

            return new BigNumber(result);
        }

        // Вычитание столбиком с займом. Требует a >= b.
        private static BigNumber Subtract(BigNumber a, BigNumber b)
        {
            if (a.CompareTo(b) < 0)
                throw new InvalidOperationException("Результат вычитания был бы отрицательным.");

            int[] result = new int[a._blocks.Length];
            int borrow = 0;

            for (int i = 0; i < a._blocks.Length; i++)
            {
                int diff = a._blocks[i] - borrow;
                if (i < b._blocks.Length) diff -= b._blocks[i];

                if (diff < 0)
                {
                    diff += Base;
                    borrow = 1;
                }
                else
                {
                    borrow = 0;
                }
                result[i] = diff;
            }

            return new BigNumber(result);
        }

        // Умножение "в столбик": каждый блок a на каждый блок b.
        private static BigNumber Multiply(BigNumber a, BigNumber b)
        {
            int[] result = new int[a._blocks.Length + b._blocks.Length];

            for (int i = 0; i < a._blocks.Length; i++)
            {
                int carry = 0;
                for (int j = 0; j < b._blocks.Length; j++)
                {
                    int cur = result[i + j] + a._blocks[i] * b._blocks[j] + carry;
                    result[i + j] = cur % Base;
                    carry = cur / Base;
                }
                result[i + b._blocks.Length] = carry;
            }

            return new BigNumber(result);
        }

        private static BigNumber Divide(BigNumber a, BigNumber b)
        {
            BigNumber remainder;
            return DivMod(a, b, out remainder);
        }

        // Деление уголком. На каждом шаге "сносим" очередной блок в остаток,
        // а цифру частного (0..Base-1) подбираем бинарным поиском.
        private static BigNumber DivMod(BigNumber a, BigNumber b, out BigNumber remainder)
        {
            if (b.IsZero)
                throw new DivideByZeroException();

            int[] quotient = new int[a._blocks.Length];
            BigNumber rem = Zero;

            for (int i = a._blocks.Length - 1; i >= 0; i--)
            {
                // rem = rem * Base + a[i]  (сдвиг на один блок)
                int[] shifted = new int[rem._blocks.Length + 1];
                shifted[0] = a._blocks[i];
                Array.Copy(rem._blocks, 0, shifted, 1, rem._blocks.Length);
                rem = new BigNumber(shifted);

                int lo = 0, hi = Base - 1;
                while (lo < hi)
                {
                    int mid = (lo + hi + 1) / 2;
                    if (Multiply(b, new BigNumber(mid)).CompareTo(rem) <= 0)
                        lo = mid;
                    else
                        hi = mid - 1;
                }

                quotient[i] = lo;
                if (lo > 0)
                    rem = Subtract(rem, Multiply(b, new BigNumber(lo)));
            }

            remainder = rem;
            return new BigNumber(quotient);
        }

        // ---------- Публичное API ----------

        public static BigNumber Pow(BigNumber value, int exponent)
        {
            if (exponent < 0)
                throw new ArgumentOutOfRangeException(nameof(exponent));

            BigNumber result = One;
            BigNumber factor = value;

            while (exponent > 0)
            {
                if ((exponent & 1) == 1) result = Multiply(result, factor);
                exponent >>= 1;
                if (exponent > 0) factor = Multiply(factor, factor);
            }
            return result;
        }

        public int CompareTo(BigNumber other)
        {
            if (ReferenceEquals(other, null)) return 1;

            if (_blocks.Length != other._blocks.Length)
                return _blocks.Length < other._blocks.Length ? -1 : 1;

            for (int i = _blocks.Length - 1; i >= 0; i--)
            {
                if (_blocks[i] != other._blocks[i])
                    return _blocks[i] < other._blocks[i] ? -1 : 1;
            }
            return 0;
        }

        public bool Equals(BigNumber other)
        {
            return !ReferenceEquals(other, null) && CompareTo(other) == 0;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BigNumber);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (int block in _blocks) hash = hash * 31 + block;
                return hash;
            }
        }

        // Старший блок печатается как есть, остальные дополняются нулями слева до 4 цифр.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_blocks[_blocks.Length - 1]);

            for (int i = _blocks.Length - 2; i >= 0; i--)
                sb.Append(_blocks[i].ToString().PadLeft(BlockLength, '0'));

            return sb.ToString();
        }

        // ---------- Операторы (только делегируют приватным методам) ----------

        public static BigNumber operator +(BigNumber a, BigNumber b) { return Add(a, b); }
        public static BigNumber operator -(BigNumber a, BigNumber b) { return Subtract(a, b); }
        public static BigNumber operator *(BigNumber a, BigNumber b) { return Multiply(a, b); }
        public static BigNumber operator /(BigNumber a, BigNumber b) { return Divide(a, b); }

        public static bool operator ==(BigNumber a, BigNumber b)
        {
            if (ReferenceEquals(a, null)) return ReferenceEquals(b, null);
            return a.Equals(b);
        }
        public static bool operator !=(BigNumber a, BigNumber b) { return !(a == b); }
        public static bool operator <(BigNumber a, BigNumber b) { return a.CompareTo(b) < 0; }
        public static bool operator >(BigNumber a, BigNumber b) { return a.CompareTo(b) > 0; }
        public static bool operator <=(BigNumber a, BigNumber b) { return a.CompareTo(b) <= 0; }
        public static bool operator >=(BigNumber a, BigNumber b) { return a.CompareTo(b) >= 0; }

        public static implicit operator BigNumber(int value) { return new BigNumber(value); }
    }
}
