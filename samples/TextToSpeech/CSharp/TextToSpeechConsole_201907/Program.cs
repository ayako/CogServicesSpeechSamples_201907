using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;

namespace TextToSpeechConsole_201907
{
    class Program
    {
        static void Main()
        {
            SynthesisToSpeakerAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static async Task SynthesisToSpeakerAsync()
        {
            var config = SpeechConfig.FromSubscription("YOUR_API_KEY", "YOUR_LOCATION");
            config.SpeechSynthesisLanguage = "ja-JP";

            // Creates a speech synthesizer using the default speaker as audio output.
            using (var synthesizer = new SpeechSynthesizer(config))
            {
                // Receive a text from console input and synthesize it to speaker.
                Console.WriteLine("Type some text that you want to speak...");
                Console.Write("> ");
                string text = Console.ReadLine();

                using (var result = await synthesizer.SpeakTextAsync(text))
                {
                    if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        Console.WriteLine($"Speech synthesized to speaker for text [{text}]");
                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }
                }
            }
        }

    }
}
