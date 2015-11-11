﻿using System;
using System.Drawing;

namespace ClassicalSharp {
	
	public sealed partial class AltTextInputWidget : Widget {
		
		Element[] elements;
		
		void InitData() {
			elements = new Element[] {
				new Element( "Colours", 8 * 3, 3, "&0█&1█&2█&3█&4█&5█&6█&7█&8█&9█&a█&b█&c█&d█&e█&f█" ),
				new Element( "Math", 16, 1, "ƒ½¼αßΓπΣσµτΦΘΩδ∞φε∩≡±≥≤⌠⌡÷≈°√ⁿ²" ),
				new Element( "Line/Box", 17, 1, "░▒▓│┤╡╢╖╕╣║╗╝╜╛┐└┴┬├─┼╞╟╚╔╩╦╠═╬╧╨╤╥╙╘╒╓╫╪┘┌█▄▌▐▀■" ),
				new Element( "Letters", 17, 1, "ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜáíóúñÑ" ),
				new Element( "Other", 16, 1, "☺☻♥♦♣♠•◘○◙♂♀♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼⌂¢£¥₧ªº¿⌐¬¡«»∙·" ),
			};
		}
		
		struct Element {
			public string Title;
			public Size TitleSize;
			public string Contents;
			public int ItemsPerRow;
			public int CharsPerItem;
			
			public Element( string title, int itemsPerRow, int charsPerItem, string contents ) {
				Title = title;
				TitleSize = Size.Empty;
				Contents = contents;
				ItemsPerRow = itemsPerRow;
				CharsPerItem = charsPerItem;
			}
		}
		
		unsafe void MeasureContentSizes( Element e, Font font, Size* sizes ) {
			string s = new String( '\0', e.CharsPerItem );
			DrawTextArgs args = new DrawTextArgs( s, font, false );
			// avoid allocating temporary strings here
			fixed( char* ptr = s ) {
				for( int i = 0; i < e.Contents.Length; i += e.CharsPerItem ) {
					for( int j = 0; j < e.CharsPerItem; j++ )
						ptr[j] = e.Contents[i + j];
					sizes[i / e.CharsPerItem] = game.Drawer2D.MeasureChatSize( ref args );
				}
			}
		}
		
		unsafe Size CalculateContentSize( Element e, Size* sizes, out Size elemSize ) {
			int wrap = e.ItemsPerRow / e.CharsPerItem;
			elemSize = Size.Empty;
			for( int i = 0; i < e.Contents.Length; i += e.CharsPerItem )
				elemSize.Width = Math.Max( elemSize.Width, sizes[i / e.CharsPerItem].Width );
			
			elemSize.Width += contentSpacing;
			elemSize.Height = sizes[0].Height + contentSpacing;
			int rows = Utils.CeilDiv( e.Contents.Length / e.CharsPerItem, wrap );
			return new Size( elemSize.Width * wrap, elemSize.Height * rows );
		}
		
		const int titleSpacing = 10, contentSpacing = 5;
		int MeasureTitles( Font font ) {
			int totalWidth = 0;
			DrawTextArgs args = new DrawTextArgs( null, font, false );
			for( int i = 0; i < elements.Length; i++ ) {
				args.Text = elements[i].Title;
				elements[i].TitleSize = game.Drawer2D.MeasureChatSize( ref args );
				elements[i].TitleSize.Width += titleSpacing;
				totalWidth += elements[i].TitleSize.Width;
			}
			return totalWidth;
		}
		
		void DrawTitles( IDrawer2D drawer, Font font ) {
			int x = 0;
			DrawTextArgs args = new DrawTextArgs( null, font, false );
			for( int i = 0; i < elements.Length; i++ ) {
				args.Text = elements[i].Title;
				FastColour col = i == selectedIndex ? new FastColour( 30, 30, 30, 200 ) :
					new FastColour( 60, 60, 60, 200 );
				Size size = elements[i].TitleSize;
				
				drawer.Clear( col, x, 0, size.Width, size.Height );
				drawer.DrawChatText( ref args, x + titleSpacing / 2, 0 );
				x += size.Width;
			}
		}
		
		unsafe void DrawContent( IDrawer2D drawer, Font font, Element e, int yOffset ) {
			string s = new String( '\0', e.CharsPerItem );
			int wrap = e.ItemsPerRow;
			DrawTextArgs args = new DrawTextArgs( s, font, false );
			
			fixed( char* ptr = s ) {
				for( int i = 0; i < e.Contents.Length; i += e.CharsPerItem ) {
					for( int j = 0; j < e.CharsPerItem; j++ )
						ptr[j] = e.Contents[i + j];
					int item = i / e.CharsPerItem;
					
					int x = (item % wrap) * elementSize.Width, y = (item / wrap) * elementSize.Height;
					y += yOffset;
					drawer.DrawChatText( ref args, x, y );
				}
			}
		}
	}
}