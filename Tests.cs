using System.Diagnostics;

namespace AnsiRenderer
{
    public static class Tests
    {
        private static string OK(bool ok) => ok ? "ok" : "failed";
        private static string Functional(bool ok) => ok ? "functional" : "failed";

        public static void CheckColorSystems()
        {
            bool ok, overall = true;

            Console.WriteLine("- Color systems:");
            ok = new Color(95, 158, 160) == ((Color)Colors.CadetBlue);
            overall = ok && overall;
            Console.WriteLine($"C# enum colors  (Colors.CadetBlue):                                             {OK(ok)}");

            ok = new Color(95, 158, 160) == Color.FromHTML("caDeTBlue");
            overall = ok && overall;
            Console.WriteLine($"String named colors  (new Color(\"cadetBlue\")):                                  {OK(ok)}");

            ok = ok && Colors.White.WithAlpha(254.0d / 255.0d) == new Color(300, 500, 234897, 254.0d / 255.0d);
            overall = ok && overall;
            Console.WriteLine($"C# RGBA assignment  (new Color(r,g,b,a)):                                       {OK(ok)}");

            ok = Colors.CadetBlue.WithAlpha(0.997) == Color.FromHTML(" rgba ( 95 , 158 , 160 , 0.997 ) ");
            ok = ok && Colors.White.WithAlpha(0.997) == Color.FromHTML(" rgba ( 300 , 500 , 234897 , 99.7 %) ");
            overall = ok && overall;
            Console.WriteLine($"HTML RGBA assignment  (new Color(\"rgba(r,g,b,a\")):                              {OK(ok)}");

            ok = Colors.CadetBlue == Color.FromHTML(" rgb ( 95 , 158 , 160 ) ");
            ok = ok && Colors.White == Color.FromHTML(" rgb ( 300 , 500 , 234897 ) ");
            overall = ok && overall;
            Console.WriteLine($"HTML RGB assignment  (new Color(\"rgb(r,g,b)\")):                                 {OK(ok)}");

            ok = new Color(128, 0, 0, 1).WithAlpha(254.0d / 255.0d) == Color.FromHSLA(0, 1, 0.25, 254.0d / 255.0d);
            ok = ok && new Color(0, 128, 128, 1).WithAlpha(254.0d / 255.0d) == Color.FromHSLA(180, 1, 0.25, 254.0d / 255.0d);
            ok = ok && new Color(128, 128, 0, 254.0d / 255.0d) == Color.FromHSLA(60, 1, 0.25, 254.0d / 255.0d);
            ok = ok && new Color(128, 128, 0, 1).WithAlpha(254.0d / 255.0d) == Color.FromHSLA(420, 1, 0.25, 254.0d / 255.0d);
            ok = ok && new Color(128, 128, 0, 1) == Color.FromHSLA(-300, 1, 0.25);
            ok = ok && new Color(128, 128, 0, 1).WithAlpha(254.0d / 255.0d) == Color.FromHSLA(-660, 1, 0.25, 254.0d / 255.0d);
            overall = ok && overall;
            Console.WriteLine($"C# HSLA assignment  (Color.FromHSLA(h,s,l,a)):                                  {OK(ok)}");

            ok = new Color(128, 0, 0, 1).WithAlpha(0.997) == Color.FromHTML(" hsla ( 0 , 1 , 0.25 , 99.7 % ) ");
            ok = ok && new Color(0, 128, 128, 1).WithAlpha(0.997) == Color.FromHTML(" hsla ( 180 , 1 , 0.25 , 99.7 % ) ");
            ok = ok && new Color(128, 128, 0, 1).WithAlpha(0.997) == Color.FromHTML(" hsla ( 60 , 1 , 0.25 , 99.7 % ) ");
            overall = ok && overall;
            Console.WriteLine($"HTML HSLA assignment  (new Color(\"hsla(h,s,l,a)\")):                             {OK(ok)}");

            ok = new Color(128, 128, 0, 1) == Color.FromHTML(" hsl ( 420 , 1 , 0.25 ) ");
            ok = ok && new Color(128, 128, 0, 1) == Color.FromHTML(" hsl ( -300 , 100 % , 0.25 ) ");
            ok = ok && new Color(128, 128, 0, 1) == Color.FromHTML(" hsl ( -660 , 1 , 25 % ) ");
            overall = ok && overall;
            Console.WriteLine($"HTML HSL assignment  (new Color(\"hsl(h,s,l)\")):                                 {OK(ok)}");

            ok = Colors.CadetBlue.WithAlpha(254.0d / 255.0d) == Color.FromUInt(0x5f9ea0fe);
            overall = ok && overall;
            Console.WriteLine($"uint assignment (Color.FromUInt(0xRRGGBBAA)):                                   {OK(ok)}");

            ok = Colors.CadetBlue.WithAlpha(254.0d / 255.0d) == Color.FromHTML("#5f9ea0fe");
            ok = ok && Colors.CadetBlue == Color.FromHTML("#5f9ea0");
            ok = ok && Color.FromUInt(0x6688aacc) == Color.FromHTML("#68ac");
            ok = ok && Color.FromUInt(0x6688aaff) == Color.FromHTML("#68a");
            overall = ok && overall;
            Console.WriteLine($"HTML hexadecimal assignment  (new Color(\"#RRGGBBAA/#RRGGBB/#RGBA/#RGB\")):       {OK(ok)}");

            ok = new Color(128, 128, 128).WithRed(192).R == new Color(192, 128, 128).R;
            ok = ok && new Color(128, 128, 128).WithGreen(192).G == new Color(128, 192, 128).G;
            ok = ok && new Color(128, 128, 128).WithBlue(192).B == new Color(128, 128, 192).B;
            overall = ok && overall;
            Console.WriteLine($"RGB setters:                                                                    {OK(ok)}");

            ok = 146 == double.Round(Color.FromHSLA(146, 0.5, 0.5).H);
            ok = ok && 60 == double.Round(Color.FromHSLA(420, 0.5, 0.5).H);
            ok = ok && 0.25 == double.Round(Color.FromHSLA(0, 0.25, 0.5).S, 2);
            ok = ok && 0.5 == double.Round(Color.FromHSLA(0, 0.25, 0.5).L, 2);
            overall = ok && overall;
            Console.WriteLine($"HSL getters:                                                                    {OK(ok)}");

            ok = double.Abs(Color.FromHSLA(0, 0.5, 0.5).WithHue(90).H - Color.FromHSLA(90, 0.5, 0.5).H) < 1;
            ok = ok && double.Abs(Color.FromHSLA(0, 0.5, 0.5).WithSaturation(0.25).S - Color.FromHSLA(0, 0.25, 0.5).S) < 0.01;
            ok = ok && double.Abs(Color.FromHSLA(0, 0.5, 0.5).WithLuminosity(0.25).L - Color.FromHSLA(0, 0.5, 0.25).L) < 0.01;
            overall = ok && overall;
            Console.WriteLine($"HSL setters:                                                                    {OK(ok)}");

            ok = Colors.Red.WithAlpha(0.80).WithOverlay(Colors.Red.WithAlpha(0.20)) == Colors.Red.WithAlpha(0.20).WithOverlay(Colors.Red.WithAlpha(0.80));
            ok = ok && Colors.Red.WithOverlay(Colors.Blue.WithAlpha(0.5)) == Colors.Purple;
            overall = ok && overall;
            Console.WriteLine($"Color compositing:                                                              {OK(ok)}");

            ok = Colors.Red.Inverted() == Colors.Cyan;
            overall = ok && overall;
            Console.WriteLine($"Color inversion:                                                                {OK(ok)}");

            Console.WriteLine($"- Overall color systems report:                                                 {Functional(overall)}");
        }

        public static void CheckRenderingSystems()
        {
            Console.CursorVisible = false;
            Console.WriteLine("Rendering sample:\n\n\n\n\n\n\n\n\n\n\n\n\n");
            int rendererBottom = Console.CursorTop;
            Renderer renderer = new();
            Color waterColor = Colors.DarkSlateBlue.WithLuminosity(0.1).WithAlpha(0.7);

            RendererObject boatRight = new(
                text:
                """
                        ▗▄          
                     ▌  ▝▛          
                     █   █▙  ▌      
                     ▌   ██▌ █      
                     █   █▀  █▋     
                     █   ▌   █      
                ▄▟▄  ▌   ▌   ▌   ▃  
                ██████████████████▃▂
                ▝████████████████▘  
                """,
                colorAreas:
                new ColorArea[]{
                    new(Colors.SaddleBrown.WithLuminosity(0.15),true,new(0,0,18,9)),
                    new(waterColor,true,new(0,8,18,1)),
                    new(Colors.SaddleBrown.WithLuminosity(0.20),true,new(5,1,1,6)),
                    new(Colors.SaddleBrown.WithLuminosity(0.20),true,new(8,0,2,7)),
                    new(Colors.SaddleBrown.WithLuminosity(0.20),true,new(13,2,1,5)),
                    new(new(140,120,100),true,new(5,2,1,1)),
                    new(new(140,120,100),true,new(5,4,1,2)),
                    new(new(140,120,100),true,new(9,2,3,3)),
                    new(new(140,120,100),true,new(13,3,2,3)),
                    new(waterColor,true,new(18,7,2,1)),
                },
                externalAlignmentX: Alignment.Start
                );
            RendererObject boatLeft = new(
                text:
                """
                          ▄▖        
                          ▜▘  ▐     
                      ▐  ▟█   █     
                      █  ██   ▐     
                     ▐█  ▀█   █     
                      █   ▐   █     
                  ▃   ▐   ▐   ▐  ▄▟▄
                ▂▃██████████████████
                  ▝████████████████▘
                """,
                colorAreas:
            new ColorArea[]{
                new(Colors.SaddleBrown.WithLuminosity(0.15),true,new(2,0,18,9)),
                new(waterColor,true,new(2,8,18,1)),
                new(Colors.SaddleBrown.WithLuminosity(0.20),true,new(14,1,1,6)),
                new(Colors.SaddleBrown.WithLuminosity(0.20),true,new(10,0,2,7)),
                new(Colors.SaddleBrown.WithLuminosity(0.20),true,new(6,2,1,5)),
                new(new(140,120,100),true,new(14,2,1,1)),
                new(new(140,120,100),true,new(14,4,1,2)),
                new(new(140,120,100),true,new(9,2,3,3)),
                new(new(140,120,100),true,new(5,3,2,3)),
                new(waterColor,true,new(0,7,2,1)),
            },
                externalAlignmentX: Alignment.End
            );

            string[] wavesFrames = new string[55];
            wavesFrames[0] = "▂▃▃▄▄▄▃▃▂▁▁▁▁▁▁▁▁▁▁▃▅▆▇▇▇▆▅▃▁▁▁▁▁▁▂▃▄▄▅▅▅▄▄▃▂▁▁▁▁▁▁▁▁▁▁";
            for (int i = 1; i < wavesFrames.Length; i++)
                wavesFrames[i] = wavesFrames[i - 1].Last() + wavesFrames[i - 1][..^1];
            RendererObject waves = new(
                animation: wavesFrames,
                colorAreas: new ColorArea[]{
                    new(waterColor,true,new(0,0,55,1)),
                },
                y: 3
            );

            RendererObject moon = new(
                animation: new string[]{
                    """
                    ▗▟▀▔  
                    █▌    
                    ▝▜▄▁  
                    """,
                    """
                    ▗▟█▀  
                    ██    
                    ▝▜█▄  
                    """,
                    """
                    ▗▟█▛  
                    ███   
                    ▝▜█▙  
                    """,
                    """
                    ▗▟██▖ 
                    ████▌ 
                    ▝▜██▘ 
                    """,
                    """
                    ▗▟██▙▖
                    ██████
                    ▝▜██▛▘
                    """,
                    """
                     ▗██▙▖
                     ▐████
                     ▝██▛▘
                    """,
                    """
                      ▜█▙▖
                       ███
                      ▟█▛▘
                    """,
                    """
                      ▀█▙▖
                        ██
                      ▄█▛▘
                    """,
                    """
                      ▔▀▙▖
                        ▐█
                      ▁▄▛▘
                    """,
                    """
                    ▗▞▔▔▚▖
                    ▌    ▐
                    ▝▚▁▁▞▘
                    """,
                },
                colorAreas: new ColorArea[]{
                    new(Colors.Goldenrod.WithLuminosity(0.8),true),
                },
                x: -2
            );

            renderer.Object = new(
                subObjects:
                new RendererObject[]{
                    new(
                        text:
                        """
                                                         *                     
                                     .                                         
                                                                               
                                        *                     .                
                                                    .                   .      
                                    .                        .                 
                            .                        .             .           
                                *             .                   .         .  
                                                *                              
                        ▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁
                                                                               
                                              . *                 .         .  
                            .   *                    .             .           
                        """,
                        colorAreas:
                        new ColorArea[]{
                            new(Colors.Black,false,new(0,0,55,13)),
                            new(Colors.White,true,new(0,0,55,13)),
                            new(Colors.Black,true,new(0,3,55,1)),
                            new(Colors.Black.WithLuminosity(0.3),true,new(0,5,55,3)),
                            new(waterColor,true,new(0,5,55,4)),
                            new(waterColor,false,new(0,5,55,3)),
                        },
                        subObjects:
                        new RendererObject[]{
                            boatRight,
                            boatLeft,
                            waves,
                            new(
                                text:
                                """
                                ╭────────────────────────────╮
                                │                            │
                                │  Night          Seascapes  │
                                │                            │
                                ╰────────────────────────────╯
                                """,
                                subObjects:new RendererObject[]{
                                    moon,
                                },
                                colorAreas:
                                new ColorArea[]{
                                    new(Colors.Black.WithAlpha(0.5),false,new(0,0,30,5)),
                                    new(Colors.SlateBlue,true,new(7,0,9,1)),
                                    new(Colors.DarkGoldenrod,true,new(-10,0,5,1)),
                                },
                                internalAlignmentX: Alignment.Center,
                                internalAlignmentY: Alignment.Center,
                                y: -2
                            ),
                        },
                        internalAlignmentX: Alignment.Center,
                        internalAlignmentY: Alignment.Center,
                        externalAlignmentX: Alignment.Left,
                        externalAlignmentY: Alignment.Bottom
                    )
                },
                y: rendererBottom - 12
            );

            bool animationStopped = false;
            int frameCount = 0;
            int waveSpeed = 1;

            int previousFrameCount = 0;
            int minFramesPerSecond = int.MaxValue;
            int maxFramesPerSecond = 0;

            void performanceSnapshotOneSecond(object? _1)
            {
                minFramesPerSecond = int.Min(minFramesPerSecond, frameCount - previousFrameCount);
                maxFramesPerSecond = int.Max(maxFramesPerSecond, frameCount - previousFrameCount);
                previousFrameCount = frameCount;
            }
            Timer timerPerformanceSnapshotOneSecond = new(performanceSnapshotOneSecond, null, 1000, 1000);

            int previousFrameCountFourth = 0;
            int minFramesPerFourth = int.MaxValue;
            int maxFramesPerFourth = 0;
            void performanceSnapshotOneFourth(object? _1)
            {
                minFramesPerFourth = int.Min(minFramesPerFourth, frameCount - previousFrameCountFourth);
                maxFramesPerFourth = int.Max(maxFramesPerFourth, frameCount - previousFrameCountFourth);
                previousFrameCountFourth = frameCount;
            }
            Timer timerPerformanceSnapshotOneFourth = new(performanceSnapshotOneFourth, null, 250, 250);

            void reverseWaves(object? _1)
            {
                waveSpeed = -1;
            }
            Timer timerReverseWaves = new(reverseWaves, null, 2500, Timeout.Infinite);

            void stopAnimation(object? _1)
            {
                animationStopped = true;
                timerPerformanceSnapshotOneSecond.Dispose();
                timerPerformanceSnapshotOneFourth.Dispose();
            }
            Timer timerStopAnimation = new(stopAnimation, null, 5050, Timeout.Infinite);

            long frameTimeMin = long.MaxValue;
            long frameTimeMax = 0;
            long frameTimeFirst = -1;
            Stopwatch stopwatch = new();

            for (int pos = 0; !animationStopped; pos = (pos + 1) % 200)
            {
                stopwatch.Start();
                boatRight.X = pos - 50;
                boatRight.ColorAreas[0] = new(Color.FromHSLA(frameCount, 0.5, 0.25), true);
                boatLeft.X = 50 - pos;
                if (pos % 2 == 0)
                    waves.Frame += waveSpeed;
                if (60 <= pos && pos < 100 && pos % 4 == 0)
                    moon.Frame++;
                renderer.Update(false);
                frameCount++;
                long time = stopwatch.ElapsedMilliseconds;
                if (frameTimeFirst == -1)
                    frameTimeFirst = time;
                else
                {
                    frameTimeMin = long.Min(frameTimeMin, time);
                    frameTimeMax = long.Max(frameTimeMax, time);
                }
                stopwatch.Reset();
            }
            boatRight.X = 0;
            boatRight.ColorAreas[0] = new(Color.FromHSLA(240, 0.5, 0.25), true);
            boatLeft.X = 0;
            moon.Frame = 0;
            renderer.Update(false);

            renderer.Object.X = 56;
            boatRight.X = 33;
            boatRight.ColorAreas[0] = new(Color.FromHSLA(60, 0.5, 0.25), true);
            boatLeft.X = -33;
            moon.Frame = -2;
            waves.Frame += 30;
            renderer.Update(false);


            Console.ResetColor();
            Console.SetCursorPosition(0, rendererBottom);
            Console.WriteLine($"\n\nFrames rendered in 5 seconds: {frameCount}");
            Console.WriteLine($"Minimum frames per 1/4s: {minFramesPerFourth}");
            Console.WriteLine($"Maximum frames per 1/4s: {maxFramesPerFourth}");
            Console.WriteLine($"Minimum frames per 1s: {minFramesPerSecond}");
            Console.WriteLine($"Maximum frames per 1s: {maxFramesPerSecond}");
            Console.WriteLine($"First frame time: {frameTimeFirst}");
            Console.WriteLine($"Minimum frame time: {frameTimeMin}");
            Console.WriteLine($"Maximum frame time: {frameTimeMax}");
            Console.CursorVisible = true;
        }

        public static void Run()
        {
            CheckColorSystems();
            Console.WriteLine();
            CheckRenderingSystems();
        }
    }
}