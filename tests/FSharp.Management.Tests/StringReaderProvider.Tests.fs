module FSharp.Management.Tests.StringReaderProviderTests

open FSharp.Management
open Expecto

type LoremIpsum = StringReader<"StringReaderProvider.Tests.data">

let [<Tests>] stringReaderTest =
    testCase "StringReader provider reads the file correctly" <| fun () ->
        let expected = """Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam scelerisque leo ut mauris ullamcorper posuere. Aliquam quis nulla dolor. Aliquam erat volutpat. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Pellentesque leo ipsum, imperdiet vehicula quam nec, egestas condimentum purus. Donec rutrum lacus ac turpis lacinia blandit. Cras sollicitudin dapibus interdum. Sed dapibus elementum turpis, at feugiat lectus interdum sed. Integer faucibus non turpis nec ullamcorper. Nam eget tincidunt ex.

Quisque vitae nulla leo. Nunc ultricies facilisis tellus, et placerat libero pulvinar et. Phasellus mollis consectetur orci in vestibulum. Integer ultricies tortor vitae condimentum dictum. Mauris id mi ante. Ut congue dolor erat, pretium aliquam neque iaculis at. Sed dictum dolor vel tellus finibus, eget tempus ligula dignissim. Ut cursus nibh magna, a feugiat risus maximus eu.

Cras accumsan vulputate neque, eu euismod tellus congue vitae. Donec at nunc est. Ut libero sapien, imperdiet nec sem vel, tincidunt feugiat turpis. Vestibulum lectus nunc, rhoncus eu tellus sed, egestas congue ex. Phasellus sed interdum lacus. Curabitur rutrum, lectus in mattis sodales, augue ex tristique enim, sit amet elementum est sem non erat. Integer sit amet libero augue. Nulla in suscipit dolor. Mauris ac porta orci, vitae varius tellus. Nunc nec consectetur dui.

Vivamus rhoncus porta nibh, quis auctor sapien fermentum quis. Maecenas nunc lectus, efficitur vitae arcu sit amet, volutpat efficitur eros. Aenean pellentesque tellus et placerat mollis. Nullam nisl lectus, bibendum sit amet varius at, scelerisque in augue. Phasellus vehicula erat sed felis iaculis rutrum. Aenean elementum ullamcorper mauris, vel aliquam diam suscipit ac. Duis eget congue felis. Proin placerat lorem odio, luctus bibendum justo dapibus et. Sed in ornare mauris. Nulla facilisi. Fusce ultricies purus id augue cursus viverra. Donec maximus risus id metus suscipit volutpat. Duis eu lacinia odio. Nullam sit amet nunc arcu.

Etiam pulvinar dui a eros ullamcorper luctus. Nam lobortis urna vitae lorem maximus, ac rhoncus felis ultricies. Morbi vestibulum ultricies urna, sed suscipit velit vehicula in. Sed iaculis velit nec ligula cursus semper. Nam sed felis vitae dui aliquet interdum sed sed eros. Donec porta ex a dolor blandit, non tempor purus molestie. Pellentesque viverra diam quis fringilla commodo. Praesent ut enim ac massa imperdiet pellentesque in eget nunc. Curabitur imperdiet erat sit amet condimentum maximus. Proin tincidunt sem sed lorem congue, nec scelerisque libero viverra."""
        Expect.equal LoremIpsum.Content expected ""