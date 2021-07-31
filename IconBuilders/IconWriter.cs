﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

public static class IconWriter
{
    public static void Write(Stream stream, IReadOnlyList<Image> images)
    {
        if (images.Any(image => image.Width > 256 || image.Height > 256))
            throw new ArgumentException("Image cannot have height or width greater than 256px.", "images");

        //
        // ICONDIR structure
        //

        WriteInt16(stream, 0); // reserved
        WriteInt16(stream, 1); // image type (icon)
        WriteInt16(stream, (short)images.Count); // number of images

        var encodedImages = images.Select(image => new
        {
            image.Width,
            image.Height,
            Bytes = EncodeImagePng(image)
        }).ToList();

        //
        // ICONDIRENTRY structure
        //

        const int iconDirSize = 6;
        const int iconDirEntrySize = 16;

        var offset = iconDirSize + (images.Count * iconDirEntrySize);

        foreach (var image in encodedImages)
        {
            stream.WriteByte((byte)image.Width);
            stream.WriteByte((byte)image.Height);
            stream.WriteByte(0); // no pallete
            stream.WriteByte(0); // reserved
            WriteInt16(stream, 0); // no color planes
            WriteInt16(stream, 32); // 32 bpp

            // image data length
            WriteInt32(stream, image.Bytes.Length);

            // image data offset
            WriteInt32(stream, offset);

            offset += image.Bytes.Length;
        }

        //
        // Image data
        //

        foreach (var image in encodedImages)
            stream.Write(image.Bytes, 0, image.Bytes.Length);
    }

    private static byte[] EncodeImagePng(Image image)
    {
        var stream = new MemoryStream();
        image.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    private static void WriteInt16(Stream stream, short s)
    {
        stream.WriteByte((byte)s);
        stream.WriteByte((byte)(s >> 8));
    }

    private static void WriteInt32(Stream stream, int i)
    {
        stream.WriteByte((byte)i);
        stream.WriteByte((byte)(i >> 8));
        stream.WriteByte((byte)(i >> 16));
        stream.WriteByte((byte)(i >> 24));
    }
}