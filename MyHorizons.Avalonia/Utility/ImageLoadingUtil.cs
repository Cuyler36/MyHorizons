using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MyHorizons.Data;
using System;

namespace MyHorizons.Avalonia.Utility
{
    public static class ImageLoadingUtil
    {
        private static readonly string[] villagerSpeciesNameLookupTable =
        {
            "ant", "bea", "brd", "bul", "cat", "cbr", "chn", "cow", "crd", "der",
            "dog", "duk", "elp", "flg", "goa", "gor", "ham", "hip", "hrs", "kal",
            "kgr", "lon", "mnk", "mus", "ocp", "ost", "pbr", "pgn", "pig", "rbt",
            "rhn", "shp", "squ", "tig", "wol", "non"
        };

        public static Bitmap LoadImageForVillager(in Villager villager)
        {
            if (villager.Species < 0x23)
            {
                // TODO: Memory could be saved here in the case of multiple villagers by caching loaded images but I'm not concerned at the moment.
                var uri = new Uri($"resm:MyHorizons.Avalonia.Resources.{villagerSpeciesNameLookupTable[villager.Species]}{villager.VariantIdx:d2}.png");
                return new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>().Open(uri));
            }
            return null;
        }
    }
}
