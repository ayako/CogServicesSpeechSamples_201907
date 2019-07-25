# Microsoft Cognitive Services を利用した 音声⇔テキスト変換サンプル (201907 版)

"人工知能 API" [Microsoft Azure Cognitive Services](https://www.microsoft.com/cognitive-services/) を使うと、音声⇔テキスト変換や翻訳 を行うエンジンをノーコーディングで利用、作成できます。

- [Speech-to-Text](https://azure.microsoft.com/ja-jp/services/cognitive-services/speech-to-text/) : 標準およびカスタム音声や会話の文字起こし
- [Text-to-Speech](https://azure.microsoft.com/ja-jp/services/cognitive-services/text-to-speech/) : 標準およびカスタムのテキスト読み上げ
- [Speech Translation](https://azure.microsoft.com/ja-jp/services/cognitive-services/speech-translation/) : 音声翻訳

# サンプルの利用方法

- Speech-to-Text (C#(UWP) | HTML/JavaScript)
- Text-to-Speech (C#(Console | UWP))


## Speech-to-Text

### C#(UWP)

[MainPage.xaml.cs](samples/SpeechToText/CSharp/SpeechToTextApp_201907/MainPage.xaml.cs) にある、YOUR_API_KEY と YOUR_LOCATION にご自分のサブスクリプションの情報(APIキー、サービスを作成したロケーション(westus, japaneast など))を入力します。

```
private async Task SpeechRecognizeAsync()
{
    var config = SpeechConfig.FromSubscription("YOUR_API_KEY", "YOUR_LOCATION");
```

### HTML/JavaScript
index.html を起動し、表示される Subscription Key と Region の欄にご自分のサブスクリプションの情報(APIキー、サービスを作成したロケーション(westus, japaneast など))を入力します。


## Text-to-Speech

### C#(Console)

[Program.cs](samples/TextToSpeech/CSharp/TextToSpeechConsole_201907/Program.cs) にある、YOUR_API_KEY と YOUR_LOCATION にご自分のサブスクリプションの情報(APIキー、サービスを作成したロケーション(westus, japaneast など))を入力します。

```
public static async Task SynthesisToSpeakerAsync()
{
    var config = SpeechConfig.FromSubscription("YOUR_API_KEY", "YOUR_LOCATION");
```

### C#(UWP)
[MainPage.xaml.cs](samples/TextToSpeech/CSharp/TextToSpeechApp_201907/MainPage.xaml.cs) にある、YOUR_API_KEY と YOUR_LOCATION にご自分のサブスクリプションの情報(APIキー、サービスを作成したロケーション(westus, japaneast など))を入力します。

```
private async void SpeakButton_Clicked(object sender, RoutedEventArgs e)
{
    var config = SpeechConfig.FromSubscription("YOUR_API_KEY", "YOUR_LOCATION");
```
