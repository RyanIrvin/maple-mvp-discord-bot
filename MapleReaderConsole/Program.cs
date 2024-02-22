using Discord;
using Discord.Commands;
using Discord.WebSocket;
using IronOcr;
using System.Drawing;
using System.Drawing.Imaging;

internal class Program
{
    private DiscordSocketClient client;
    private const string IMAGEPATH = "C:\\Users\\Sticky\\source\\repos\\MapleReaderConsole\\MapleReaderConsole\\bin\\Debug\\net6.0\\image.png";
    private int Frequency = 60000;
    private string lastmatch;

    private static Task Main(string[] args)
    {
        Thread.Sleep(5000);
        return new Program().MainAsync();
    }

    public async Task MainAsync()
    {
        IronTesseract ocr = new IronTesseract();
        client = new DiscordSocketClient();
        client.Log += Log;
        string token = "MTEwODE2NzQ5MzMwMzkzNTA0Nw.Geepk7.RwwGnfeGfAY2kC4lXu8denduNs4QjyfwvuNhXg";

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        while (true)
        {
            Run(ocr);
            Thread.Sleep(Frequency);
        }
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private async void Run(IronTesseract ocr)
    {
        var result = String.Empty;

        SaveScreenshot();

        using (var ocrInput = new OcrInput())
        {
            ocrInput.AddImage("image.png");
            ocrInput.Sharpen();

            var ocrResult = ocr.Read(ocrInput);
            result = ocrResult.Text;
        }

        if (CheckForMvp(result))
        {
            Frequency = 300000;
        }
        else
            Frequency = 60000;
    }

    private async void SaveScreenshot()
    {
        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 200, 406, 167);
        Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
        Graphics graphics = Graphics.FromImage(bmp);

        graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
        System.Drawing.Size newSize = new System.Drawing.Size((int)Math.Round(bmp.Width * 2.1), (int)Math.Round(bmp.Height * 2.1));

        bmp = new Bitmap(bmp, newSize);
        bmp.Save("image.png", System.Drawing.Imaging.ImageFormat.Png);
    }

    private bool CheckForMvp(string result)
    {
        List<string> results = result.ToLower().Split("\n").ToList();
        string[] keywords = { "mvp", "50%", "bonus", "atmospheric", "xx", "cc1", "cc 1", "cc2", "cc 2", "cc3", "cc 3", "ch1", "ch 1", "ch2", "ch 2", "ch3", "ch 3" };
        string[] ignorekeywords = { "mvps", "mvp pls", "any mvp", "mpe", "born to be"};

        foreach (string line in results)
        {
            if (keywords.Any(line.Contains))
            {
                Console.WriteLine(line);

                if (!ignorekeywords.Any(line.Contains))
                {
                    string newMatch = line.Substring(0, 4);

                    if (!newMatch.Equals(lastmatch))
                    {
                        lastmatch = newMatch;
                        PostMvpMessage(line);
                        return true;
                    }
                    else
                        Console.WriteLine($"{line} - already posted?");
                }
                else
                    Console.WriteLine($"{line} - ignoring");
            }
        }

        return false;
    }

    private async void PostMvpMessage(string line)
    {
        var test = await client.GetGuild(768844173527875674).GetTextChannel(1108580079191199845).SendFileAsync(IMAGEPATH, $"MVP?\nThe bot saw: {line}");
        Thread.Sleep(180000);
        await test.DeleteAsync();
    }
}