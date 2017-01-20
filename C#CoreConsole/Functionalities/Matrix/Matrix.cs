using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication.Functions.Matrix
{
    public static class Matrix
    {
        static Random rand = new Random();
        
        static event Action ChangePatternEvent;

        static char AsciiCharacter
        {
            get
            {
                int t = rand.Next(10);
                if (t <= 2) return (char)('0' + rand.Next(10));
                else if (t <= 4) return (char)('a' + rand.Next(27));
                else if (t <= 6) return (char)('A' + rand.Next(27));
                else return (char)(rand.Next(32, 255));
            }
        }
        
        public static void Start(CancellationToken ct)
        {
            U.CheckEnvironementAsync().Wait();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Clear();
            C.WL(@"
            _|      _|              _|                _|            
            _|_|  _|_|    _|_|_|  _|_|_|_|  _|  _|_|      _|    _|  
            _|  _|  _|  _|    _|    _|      _|_|      _|    _|_|    
            _|      _|  _|    _|    _|      _|        _|  _|    _|  
            _|      _|    _|_|_|      _|_|  _|        _|  _|    _|");
            var intro = "The Matrix Has You...";
            for(int i = 0; i < intro.Length ; i++){ C.Write(intro[i].ToString()); Thread.Sleep(30);}
            C.Key();
            Console.CursorVisible = false;
            
            int width, height;
            
            int[] y;

            Initialize(out width, out height, out y);

            Task.Factory.StartNew(() => {
                ChangePatternEvent += () => {
                    Initialize(out width, out height, out y);
                };
                // do the Matrix effect
                while ( true )
                    Generate(width, height, y);
            }, ct).Wait();
        }

        private static void Generate(int width, int height, int[] y)
        {
            for (int x = 0 ; x < width ; ++x )
            {
                Console.ForegroundColor = U.OSX ? ConsoleColor.Gray    
                                                : ConsoleColor.Green;
                C.Cursor(x, y[x]);
                C.WriteChar(ProcessChar());

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                C.Cursor(x, inScreenYPosition(y[x] - 2, height));
                C.WriteChar(ProcessChar());

                C.Cursor(x, inScreenYPosition(y[x] - 18, height));
                C.WriteChar(' ');

                // increment y
                y[x] = inScreenYPosition(y[x] + 1, height);
            }
            if(U.OSX)Thread.Sleep(40);
            CheckInput();
        }

        private static void CheckInput()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey().Key;
                if (key == ConsoleKey.F1)
                    if(ChangePatternEvent != null)
                        ChangePatternEvent();
                if (key == ConsoleKey.F5) Console.Clear();
                if (key == ConsoleKey.F8) C.Key();
            }
        }

        //when y position is off screen
        public static int inScreenYPosition(int yPosition, int height)
        {
            if (yPosition < 0) return yPosition + height;
            else if (yPosition < height) return yPosition;
            else return 0;
        }

        private static void Initialize(out int width, out int height, out int[] y)
        {
            height = Console.WindowHeight;
            width = Console.WindowWidth - 1;
            // starting y positions of bright green characters
            y = new int[width];
            for ( int x = 0 ; x < width ; ++x )
            {
                y[x] = rand.Next(height);
            }
        }

        private static char ProcessChar()
        {
            if(U.OSX)
                return Encoding.ASCII.GetChars(Encoding.ASCII.GetBytes(new char[1]{AsciiCharacter}))[0];
            else
                return AsciiCharacter;
        }

    }
}