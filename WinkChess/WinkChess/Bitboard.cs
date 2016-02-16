using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkChess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Masks
    {
        private static bool inited = false;

        private static Dictionary<K, V>[] CustomDict<K, V>(int length)
        {
            Dictionary<K, V>[] output = new Dictionary<K, V>[length];
            for (int i = 0; i < length; i++)
            {
                output[i] = new Dictionary<K, V>();
            }
            return output;
        }

        private static List<T>[] CustomList<T>(int length)
        {
            List<T>[] output = new List<T>[length];
            for (int i = 0; i < length; i++)
            {
                output[i] = new List<T>();
            }
            return output;
        }

        public static long[] wp_masks_all = new long[64];
        public static long[] wp_masks_diff = new long[64];
        public static long[] bp_masks_all = new long[64];
        public static long[] bp_masks_diff = new long[64];
        public static Dictionary<long, List<short>>[] wp_moves = CustomDict<long, List<short>>(64);
        public static Dictionary<long, List<short>>[] bp_moves = CustomDict<long, List<short>>(64);

        public static long[] k_masks = new long[64];
        public static Dictionary<long, List<short>>[] k_moves = CustomDict<long, List<short>>(64);

        public static long[] n_masks = new long[64];
        public static Dictionary<long, List<short>>[] n_moves = CustomDict<long, List<short>>(64);

        /// <summary>
        /// h_masks, etc. applied to the bitboard state to get the position of 
        /// enemy and friendly pieces stored in a long.
        /// h_moves_friendly, etc. used to generate a long representing all the
        /// possible move locations given the positions of enemy and friendly
        /// pieces returned from the previous long &'ed.
        /// h_moves_list, etc. returns a list of start, end coordinates of possible
        /// moves according to the & of the friendly and enemy piece movement
        /// possibilities.
        /// 
        /// h_moves_list[
        ///     h_moves_friendly[index][
        ///         white & h_masks[index]
        ///     ] & h_moves_enemy[index][
        ///         black & h_masks[index]
        ///     ]
        /// ]
        /// </summary>

        public static long[] h_masks = new long[64];
        public static long[] v_masks = new long[64];
        public static long[] d1_masks = new long[64];
        public static long[] d2_masks = new long[64];
        public static Dictionary<long, long>[] h_moves_friendly = CustomDict<long, long>(64);
        public static Dictionary<long, long>[] h_moves_enemy = CustomDict<long, long>(64);
        public static Dictionary<long, long>[] v_moves_friendly = CustomDict<long, long>(64);
        public static Dictionary<long, long>[] v_moves_enemy = CustomDict<long, long>(64);
        public static Dictionary<long, long>[] d1_moves_friendly = CustomDict<long, long>(64);
        public static Dictionary<long, long>[] d1_moves_enemy = CustomDict<long, long>(64);
        public static Dictionary<long, long>[] d2_moves_friendly = CustomDict<long, long>(64);
        public static Dictionary<long, long>[] d2_moves_enemy = CustomDict<long, long>(64);
        public static Dictionary<long, List<short>>[] h_moves_list = CustomDict<long, List<short>>(64);
        public static Dictionary<long, List<short>>[] v_moves_list = CustomDict<long, List<short>>(64);
        public static Dictionary<long, List<short>>[] d1_moves_list = CustomDict<long, List<short>>(64);
        public static Dictionary<long, List<short>>[] d2_moves_list = CustomDict<long, List<short>>(64);

        //zobrist hashing random number arrays
        public static long[,] zobrist = new long[64, 13];
        public static long[] zobrist_special = new long[64];

        //bit masks for isolating specific bits
        public static long[] bit_mask = new long[64];
        public static long[] inv_bit_mask = new long[64];

        //king and knight move destinations for check testing
        public static List<byte>[] n_endpoints = CustomList<byte>(64);
        public static List<byte>[] k_endpoints = CustomList<byte>(64);

        //king safety check masks
        public static long[] king_safety_masks = new long[64];
        public static Dictionary<long, int>[] king_safety_counts = CustomDict<long, int>(64);

        //number of rook moves given a 64-bit integer representing possible vertical moves
        public static Dictionary<long, int>[] v_move_count = CustomDict<long, int>(64);
        public static Dictionary<long, int>[] h_move_count = CustomDict<long, int>(64);
        //number of bishop move given a 64-bit integer representing possible diagonal moves
        public static Dictionary<long, int>[] d1_move_count = CustomDict<long, int>(64);
        public static Dictionary<long, int>[] d2_move_count = CustomDict<long, int>(64);

        //passed pawn detection masks
        public static long[] wp_masks_passed = new long[64];
        public static long[] bp_masks_passed = new long[64];
        //split up pawns along a file into distinct longs
        public static Dictionary<long, List<long>>[] wp_split = CustomDict<long, List<long>>(8);
        public static Dictionary<long, List<long>>[] bp_split = CustomDict<long, List<long>>(8);

        /// <summary>
        /// Generates all the bitboard masks and arrays for an instance.
        /// </summary>

        public static void PrintBin(long value)
        {
            string text = Convert.ToString(value, 2);
            while (text.Length < 64)
            {
                text = text.Insert(0, "0");
            }
            System.Console.WriteLine("+--------+");
            for (int row = 0; row < 8; row++)
            {
                string output = "|";
                for (int i = 7; i >= 0; i--)
                {
                    output += text[row * 8 + i];

                }
                output += "|";
                System.Console.WriteLine(output);
            }
            System.Console.WriteLine("+--------+");
        }

        public static void Init()
        {
            if (!inited)
            {
                inited = true;
                RookMasks();
                BishopMasks();
                KnightKingMasksMoves();
                PawnMasks();
                RookMoves();
                BishopMoves();
                PawnMoves();
                Zobrist();
                BitMasks();
                KingSafetyMasks();
                FileDiagonalCounts();
            }
        }

        private static void RookMasks()
        {
            //template to apply bitshifts to
            long V_TEMP = 0x101010101010101;
            long H_TEMP = 0xff;

            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    //integer index
                    int index = row * 8 + column;
                    //rows and columns shifted over to row and column
                    Masks.v_masks[index] = V_TEMP << column;
                    Masks.h_masks[index] = H_TEMP << (row * 8);
                }
            }
        }

        private static void BishopMasks()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    //set x, y to row, column for easy math
                    int x = row;
                    int y = column;

                    //index for piece position
                    int index = x * 8 + y;

                    long d1 = 0;
                    long d2 = 0;
                    for (int step = -7; step < 8; step++)
                    {
                        //if is in lineup for d1
                        if (0 <= (x + step) && (x + step) < 8 && 0 <= (y + step) && (y + step) < 8)
                        {
                            d1 |= (long)1 << ((x + step) * 8 + (y + step));
                        }

                        //if is in lineup for d2
                        if (0 <= (x + step) && (x + step) < 8 && 0 <= (y - step) && (y - step) < 8)
                        {
                            d2 |= (long)1 << ((x + step) * 8 + (y - step));
                        }
                    }
                    Masks.d1_masks[index] = d1;
                    Masks.d2_masks[index] = d2;
                }
            }
        }

        private static void KnightKingMasksMoves()
        {
            int[,] km = new int[8, 2] { { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 } };
            int[,] nm = new int[8, 2] { { -2, -1 }, { -2, 1 }, { -1, -2 }, { -1, 2 }, { 1, -2 }, { 1, 2 }, { 2, -1 }, { 2, 1 } };
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    int x = row;
                    int y = column;

                    int index = x * 8 + y;

                    long king = 0;
                    List<int> king_list = new List<int>();
                    for (int i = 0; i < 8; i++)
                    {
                        int dx = km[i, 0];
                        int dy = km[i, 1];
                        if (0 <= (x + dx) && (x + dx) < 8 && 0 <= (y + dy) && (y + dy) < 8)
                        {
                            king |= (long)1 << ((x + dx) * 8 + (y + dy));
                            king_list.Add((byte)((x + dx) * 8 + (y + dy)));
                            k_endpoints[index].Add((byte)((x + dx) * 8 + (y + dy)));
                        }
                    }
                    for (int combo = 0; combo < Math.Pow(2, king_list.Count); combo++)
                    {
                        List<short> king_moves = new List<short>();
                        long king_key = 0;
                        for (int pos = 0; pos < king_list.Count; pos++)
                        {
                            bool bitstate = ((combo >> pos) & 1) == 1;
                            if (bitstate)
                            {
                                king_key |= (long)1 << king_list.ElementAt(pos);
                                king_moves.Add((short)((index << 6) | king_list.ElementAt(pos)));
                            }
                        }
                        if (!Masks.k_moves[index].ContainsKey(king_key))
                        {
                            Masks.k_moves[index].Add(king_key, king_moves);
                        }
                    }
                    Masks.k_masks[index] = king;

                    long knight = 0;
                    List<int> knight_list = new List<int>();
                    for (int i = 0; i < 8; i++)
                    {
                        int dx = nm[i, 0];
                        int dy = nm[i, 1];
                        if (0 <= (x + dx) && (x + dx) < 8 && 0 <= (y + dy) && (y + dy) < 8)
                        {
                            knight |= (long)1 << ((x + dx) * 8 + (y + dy));
                            knight_list.Add((byte)((x + dx) * 8 + (y + dy)));
                            n_endpoints[index].Add((byte)((x + dx) * 8 + (y + dy)));
                        }
                    }
                    for (int combo = 0; combo < Math.Pow(2, knight_list.Count); combo++)
                    {
                        List<short> knight_moves = new List<short>();
                        long knight_key = 0;
                        for (int pos = 0; pos < knight_list.Count; pos++)
                        {
                            bool bitstate = ((combo >> pos) & 1) == 1;
                            if (bitstate)
                            {
                                knight_key |= (long)1 << knight_list.ElementAt(pos);
                                knight_moves.Add((short)((index << 6) | knight_list.ElementAt(pos)));
                            }
                        }
                        if (!Masks.n_moves[index].ContainsKey(knight_key))
                        {
                            Masks.n_moves[index].Add(knight_key, knight_moves);
                        }
                    }
                    Masks.n_masks[index] = knight;
                }
            }
        }

        private static void PawnMasks()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    int x = row;
                    int y = column;

                    int index = row * 8 + column;

                    //white pawns
                    if (row != 7)
                    {
                        //white pawn forward moves
                        long wp_all = (long)1 << (row * 8 + column + 8);
                        if (row == 1)
                        {
                            wp_all |= (long)1 << (row * 8 + column + 16);
                        }
                        Masks.wp_masks_all[index] = wp_all;

                        //white pawn capture moves
                        long wp_diff = 0;
                        if (column != 0)
                        {
                            wp_diff |= (long)1 << (row * 8 + column + 7);
                        }
                        if (column != 7)
                        {
                            wp_diff |= (long)1 << (row * 8 + column + 9);
                        }
                        Masks.wp_masks_diff[index] = wp_diff;
                    }

                    //black pawns
                    if (row != 0)
                    {
                        //black pawn forward moves
                        long bp_all = (long)1 << (row * 8 + column - 8);
                        if (row == 6)
                        {
                            bp_all |= (long)1 << (row * 8 + column - 16);
                        }
                        Masks.bp_masks_all[index] = bp_all;

                        //black pawn capture moves
                        long bp_diff = 0;
                        if (column != 0)
                        {
                            bp_diff |= (long)1 << (row * 8 + column - 9);
                        }
                        if (column != 7)
                        {
                            bp_diff |= (long)1 << (row * 8 + column - 7);
                        }
                        Masks.bp_masks_diff[index] = bp_diff;
                    }
                }
            }
        }

        private static void RookMoves()
        {
            for (long combo = 0; combo < 256; combo++)
            {
                for (int row = 0; row < 8; row++)
                {
                    for (int column = 0; column < 8; column++)
                    {
                        int x = row;
                        int y = column;
                        int index = row * 8 + column;

                        //generate binary keys to be used in dictionary to retrieve list of moves
                        long horizontal_key = (long)combo << (row * 8);
                        long vertical_key = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            vertical_key |= (long)((combo >> i) & 1) << (i * 8 + column);
                        }

                        //generate binary move combinations
                        long horizontal_enemy = 0;
                        long horizontal_friendly = 0;
                        long vertical_enemy = 0;
                        long vertical_friendly = 0;
                        //horizontal moves
                        for (int pos = column - 1; pos >= 0; pos--)
                        {
                            if (((combo >> pos) & 1) == 1)
                            {
                                horizontal_enemy |= (long)1 << (pos + row * 8);
                                break;
                            }
                            else
                            {
                                horizontal_enemy |= (long)1 << (pos + row * 8);
                                horizontal_friendly |= (long)1 << (pos + row * 8);
                            }
                        }
                        for (int pos = column + 1; pos < 8; pos++)
                        {
                            if (((combo >> pos) & 1) == 1)
                            {
                                horizontal_enemy |= (long)1 << (pos + row * 8);
                                break;
                            }
                            else
                            {
                                horizontal_enemy |= (long)1 << (pos + row * 8);
                                horizontal_friendly |= (long)1 << (pos + row * 8);
                            }
                        }
                        Masks.h_moves_enemy[index].Add(horizontal_key, horizontal_enemy);
                        Masks.h_moves_friendly[index].Add(horizontal_key, horizontal_friendly);
                        //vertical moves
                        for (int pos = row - 1; pos >= 0; pos--)
                        {
                            if (((combo >> pos) & 1) == 1)
                            {
                                vertical_enemy |= (long)1 << (pos * 8 + column);
                                break;
                            }
                            else
                            {
                                vertical_enemy |= (long)1 << (pos * 8 + column);
                                vertical_friendly |= (long)1 << (pos * 8 + column);
                            }
                        }
                        for (int pos = row + 1; pos < 8; pos++)
                        {
                            if (((combo >> pos) & 1) == 1)
                            {
                                vertical_enemy |= (long)1 << (pos * 8 + column);
                                break;
                            }
                            else
                            {
                                vertical_enemy |= (long)1 << (pos * 8 + column);
                                vertical_friendly |= (long)1 << (pos * 8 + column);
                            }
                        }
                        Masks.v_moves_enemy[index].Add(vertical_key, vertical_enemy);
                        Masks.v_moves_friendly[index].Add(vertical_key, vertical_friendly);

                        //get list of moves given available moves on combo
                        List<short> horizontal_moves = new List<short>();
                        List<short> vertical_moves = new List<short>();
                        for (var i = 0; i < 8; i++)
                        {
                            if (((combo >> i) & 1) == 1)
                            {
                                short horizontal_coords = (short)((index << 6) | (i + row * 8));
                                horizontal_moves.Add(horizontal_coords);
                                short vertical_coords = (short)((index << 6) | (column + i * 8));
                                vertical_moves.Add(vertical_coords);
                            }
                        }
                        //add lists to list tables
                        Masks.v_moves_list[index].Add(vertical_key, vertical_moves);
                        Masks.h_moves_list[index].Add(horizontal_key, horizontal_moves);
                    }
                }
            }
        }

        private static void BishopMoves()
        {
            for (long combo = 0; combo < 256; combo++)
            {
                //generate combo block
                long combo_block = 0;
                for (int pos = 0; pos < 8; pos++)
                {
                    long combo_state = (combo >> pos) & 1;
                    combo_block |= (combo_state * 0xff) << (pos * 8);
                }

                for (int row = 0; row < 8; row++)
                {
                    int x = row;

                    //generate enemy and friendly integers
                    long enemy = 0;
                    long friendly = 0;
                    for (int pos = row - 1; pos >= 0; pos--)
                    {
                        enemy |= (long)1 << pos;
                        if (((combo >> pos) & 1) == 1)
                        {
                            break;
                        }
                        else
                        {
                            friendly |= (long)1 << pos;
                        }
                    }
                    for (int pos = row + 1; pos < 8; pos++)
                    {
                        enemy |= (long)1 << pos;
                        if (((combo >> pos) & 1) == 1)
                        {
                            break;
                        }
                        else
                        {
                            friendly |= (long)1 << pos;
                        }
                    }

                    //convert combo in binary to 8-bit blocks (1 --> 0xff, 0 --> 0x00)
                    //diagonal bit masks will be applied later for simplicity
                    long enemy_block = 0;
                    long friendly_block = 0;

                    //distribute enemy and friendly to the block formation
                    for (int pos = 0; pos < 8; pos++)
                    {
                        long enemy_state = (enemy >> pos) & 1;
                        enemy_block |= (enemy_state * 0xff) << (pos * 8);

                        long friendly_state = (friendly >> pos) & 1;
                        friendly_block |= (friendly_state * 0xff) << (pos * 8);
                    }

                    for (int column = 0; column < 8; column++)
                    {
                        int y = column;
                        int index = x * 8 + y;

                        //merge key, friendly and enemy blocks with d1 and d2 masks
                        long d1_key = Masks.d1_masks[index] & combo_block;
                        long d2_key = Masks.d2_masks[index] & combo_block;
                        long d1_enemy = Masks.d1_masks[index] & enemy_block;
                        long d2_enemy = Masks.d2_masks[index] & enemy_block;
                        long d1_friendly = Masks.d1_masks[index] & friendly_block;
                        long d2_friendly = Masks.d2_masks[index] & friendly_block;

                        //generate list of move coordinates
                        List<short> d1_moves = new List<short>();
                        List<short> d2_moves = new List<short>();
                        for (byte i = 0; i < 64; i++)
                        {
                            if (((d1_key >> i) & 1) == 1)
                            {
                                d1_moves.Add((short)((index << 6 | i)));
                            }
                            if (((d2_key >> i) & 1) == 1)
                            {
                                d2_moves.Add((short)((index << 6 | i)));
                            }
                        }

                        //set final values to Dictionaries
                        if (!Masks.d1_moves_enemy[index].ContainsKey(d1_key))
                        {
                            Masks.d1_moves_enemy[index].Add(d1_key, d1_enemy);
                            Masks.d1_moves_friendly[index].Add(d1_key, d1_friendly);
                            Masks.d1_moves_list[index].Add(d1_key, d1_moves);
                        }
                        if (!Masks.d2_moves_enemy[index].ContainsKey(d2_key))
                        {
                            Masks.d2_moves_enemy[index].Add(d2_key, d2_enemy);
                            Masks.d2_moves_friendly[index].Add(d2_key, d2_friendly);
                            Masks.d2_moves_list[index].Add(d2_key, d2_moves);
                        }

                    }
                }
            }
        }

        private static void PawnMoves()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    int index = row * 8 + column;

                    //white pawns
                    if (row != 7)
                    {
                        for (long combo = 0; combo < 16; combo++)
                        {
                            long wp_key = 0;
                            List<short> wp_moves_list = new List<short>();

                            //for all, TRUE REPRESENTS A PIECE BEING THERE!!!
                            bool front = (combo & 1) == 1;
                            bool front2 = ((combo >> 1) & 1) == 1;
                            bool right = ((combo >> 2) & 1) == 1;
                            bool left = ((combo >> 3) & 1) == 1;

                            if (!front)
                            {
                                wp_moves_list.Add((short)((index << 6) | (index + 8)));
                                if (row == 1)
                                {
                                    if (!front2)
                                    {
                                        wp_moves_list.Add((short)((index << 6) | (index + 16)));
                                    }
                                    else
                                    {
                                        wp_key |= (long)1 << (index + 16);
                                    }
                                }
                            }
                            else
                            {
                                wp_key |= (long)1 << (index + 8);
                                if (front2 && row == 1)
                                {
                                    wp_key |= (long)1 << (index + 16);
                                }
                            }
                            if (column != 7 && right)
                            {
                                wp_moves_list.Add((short)((index << 6) | (index + 9)));
                                wp_key |= (long)1 << (index + 9);
                            }
                            if (column != 0 && left)
                            {
                                wp_moves_list.Add((short)((index << 6) | (index + 7)));
                                wp_key |= (long)1 << (index + 7);
                            }
                            if (!wp_moves[index].ContainsKey(wp_key))
                            {
                                wp_moves[index].Add(wp_key, wp_moves_list);
                            }
                        }
                    }

                    //black pawns
                    if (row != 0)
                    {
                        for (long combo = 0; combo < 16; combo++)
                        {
                            long bp_key = 0;
                            List<short> bp_moves_list = new List<short>();
                            //for all, TRUE REPRESENTS A PIECE BEING THERE!!!
                            bool front = (combo & 1) == 1;
                            bool front2 = ((combo >> 1) & 1) == 1;
                            bool right = ((combo >> 2) & 1) == 1;
                            bool left = ((combo >> 3) & 1) == 1;

                            if (!front)
                            {
                                bp_moves_list.Add((short)((index << 6) | (index - 8)));
                                if (row == 6)
                                {
                                    if (!front2)
                                    {
                                        bp_moves_list.Add((short)((index << 6) | (index - 16)));
                                    }
                                    else
                                    {
                                        bp_key |= (long)1 << (index - 16);
                                    }
                                }
                            }
                            else
                            {
                                bp_key |= (long)1 << (index - 8);
                                if (row == 6 && front2)
                                {
                                    bp_key |= (long)1 << (index - 16);
                                }
                            }
                            if (column != 7 && right)
                            {
                                bp_moves_list.Add((short)((index << 6) | (index - 7)));
                                bp_key |= (long)1 << (index - 7);
                            }
                            if (column != 0 && left)
                            {
                                bp_moves_list.Add((short)((index << 6) | (index - 9)));
                                bp_key |= (long)1 << (index - 9);
                            }
                            if (!bp_moves[index].ContainsKey(bp_key))
                            {
                                bp_moves[index].Add(bp_key, bp_moves_list);
                            }
                        }
                    }
                }
            }
        }

        private static void Zobrist()
        {
            Random rand = new Random();
            for (int pos = 0; pos < 64; pos++)
            {
                zobrist[pos, 0] = 0;
                for (int piece = 1; piece < 13; piece++)
                {
                    byte[] buf = new byte[8];
                    rand.NextBytes(buf);
                    zobrist[pos, piece] = BitConverter.ToInt64(buf, 0);
                }
            }
        }

        private static void BitMasks()
        {
            for (int i = 0; i < 64; i++)
            {
                bit_mask[i] = (long)1 << i;
                inv_bit_mask[i] = ~((long)1 << i);
            }
        }

        private static void KingSafetyMasks()
        {
            int[,] steps = new int[24, 2]
            {
            {-2,-2 }, {-2,-1 }, {-2,0 }, {-2,1 }, {-2,2 },
            {-1,-2 }, {-1,-1 }, {-1,0 }, {-1,1 }, {-1,2 },
            {-0,-2 }, {0,-1 },          {0,1 }, {0,2 },
            {1,-2 }, {1,-1 }, {1,0 }, {1,1 }, {1,2 },
            {2,-2 }, {2,-1 }, {2,0 }, {2,1 }, {2,2 },
            };

            //generate masks
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {

                    long mask = 0;

                    for (int i = 0; i < steps.GetLength(0); i++)
                    {
                        int dx = x + steps[i, 0];
                        int dy = y + steps[i, 1];

                        if (0 <= dx && dx < 8 && 0 <= dy && dy < 8)
                            mask |= (long)1 << (dx * 8 + dy);
                    }

                    king_safety_masks[x * 8 + y] = mask;
                }
            }

            //generate possibilities for total on bits for all combinations
        }

        private static void FileDiagonalCounts()
        {
            for (int index = 0; index < 64; index++)
            {
                foreach (KeyValuePair<long, List<short>> pair in d1_moves_list[index])
                {
                    d1_move_count[index].Add(pair.Key, pair.Value.Count);
                }

                foreach (KeyValuePair<long, List<short>> pair in d2_moves_list[index])
                {
                    d2_move_count[index].Add(pair.Key, pair.Value.Count);
                }

                foreach (KeyValuePair<long, List<short>> pair in v_moves_list[index])
                {
                    v_move_count[index].Add(pair.Key, pair.Value.Count);
                }

                foreach (KeyValuePair<long, List<short>> pair in h_moves_list[index])
                {
                    h_move_count[index].Add(pair.Key, pair.Value.Count);
                }

            }
        }
    }

    public class Bitboard
    {
        /// <summary>
        /// Numbers that represent different pieces. 0 is empty by default.
        /// </summary>
        public enum Pieces : byte { empty = 0,
            wpawn, wknight, wbishop, wrook, wqueen, wking,
            bpawn, bknight, bbishop, brook, bqueen, bking };
        /// <summary>
        /// Flags that represent castling status. "|" them together to create
        /// a castling byte.
        /// </summary>
        [Flags] public enum Castling : byte {
            whiteKingside = 0x01, whiteQueenside = 0x02, blackKingside = 0x04, blackQueenSide = 0x08 };

        /// <summary>
        /// Get piece at board position.
        /// </summary>
        /// <param name="file">
        /// File of coordinates (i.e. "a" = 0, "h" = 7).
        /// </param>
        /// <param name="rank">
        /// Rank of coordinates (i.e. "1" = 0, "8" = 7).
        /// </param>
        /// <returns></returns>
        public byte this[int file, int rank]
        {
            get { return bitboard[rank * 8 + file]; }
        }

        /// <summary>
        /// 64-bit integer bitboard representation of different classes of
        /// pieces.
        /// </summary>
        long white, black, all, wpawns, bpawns;
        /// <summary>
        /// Byte array used to store specific piece locations.
        /// </summary>
        byte[] bitboard;
        /// <summary>
        /// Castling status of position.
        /// </summary>
        byte castling;
        /// <summary>
        /// En passant status of position.
        /// </summary>
        byte ep;

        bool color;
        
        public Bitboard(byte[] bitboard, bool color, byte castling, byte ep)
        {
            this.bitboard = bitboard;
            this.color = color;
            this.castling = castling, this.ep = ep;
        }
    }
}
