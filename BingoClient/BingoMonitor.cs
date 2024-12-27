using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.BingoUI;
using Microsoft.Xna.Framework;
using Monocle;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// this file is for static methods to query the game state
// this includes methods to query BingoClient.ModSaveData

namespace Celeste.Mod.BingoClient {
    public static class BingoMonitor {
        #region data
        public static string[] TheoCutscenes = { "cutscene:1:6zb", "cutscene:2:end_2", "cutscene:3:09-d", "cutscene:5:search" };
        public static string[] KeysSearch = { "key:5:0:d-15:216", "key:5:0:d-04:39", "key:5:0:d-04:14" };
        public static string[] KeysFW = { "key:10:0:d-01:261", "key:10:0:d-02:70", "key:10:0:d-03:315", "key:10:0:d-04:444", "key:10:0:d-05:593" };
        public static string[] Keys3A = { "key:3:0:s3:15", "key:3:0:02-b:32", "key:3:0:07-b:2", "key:3:0:09-b:13", "key:3:0:02-c:1" };
        public static string[] Keys5A = { "key:5:0:a-08:55", "key:5:0:b-04:3", "key:5:0:d-15:216", "key:5:0:d-04:39", "key:5:0:d-04:14" };
        public static string[] Keys5B = { "key:5:1:b-02:221", "key:5:1:b-02:219" };
        public static string[] KeysAll = KeysFW.Concat(Keys3A).Concat(Keys5A).Concat(Keys5B).Concat(new[] {"key:7:0:f-07:712"}).ToArray();
        public static Dictionary<string, Func<float>> Objectives = new Dictionary<string, Func<float>> {
            { "Talk to Theo in Crossing", () => HasFlag("cutscene:1:6zb") },
            { "Complete 1A Start without jumping", null }, // generated
            { "All Berries in Start of 1A (6)", () => HasCheckpointBerries(1, 0) },
            { "All Berries in Chasm (5)", () => HasCheckpointBerries(1, 2) },
            { "Get a 1-Up in 1A", () => HasN1upsInChapter(1, 1) },
            { "Forsaken City Blue Heart", () => HasHeart(1, 0) },
            { "Complete 1A Start without dashing", null }, // generated
            { "Forsaken City Cassette", () => HasCassette(1) },
            { "Old Site Blue Heart", () => HasHeart(2, 0) },
            { "Get two 1-Ups", () => HasN1ups(2) },
            { "10 Berries in 1A", () => HasNBerriesInChapter(10, 1) },
            { "12 Berries in 1A", () => HasNBerriesInChapter(12, 1) },
            { "Talk to Theo in Awake", () => HasFlag("cutscene:2:end_2") },
            { "All Berries in Awake (1)", () => HasCheckpointBerries(2, 2) },
            { "All Berries in Intervention (8)", () => HasCheckpointBerries(2, 1) },
            { "Complete Awake without dashing", null }, // generated
            { "All Berries in Start of 2A (9)", () => HasCheckpointBerries(2, 0) },
            { "Old Site Cassette", () => HasCassette(2) },
            { "Complete Chasm without dashing", null }, // generated
            { "5 Berries in 3 Chapters", () => HasNBerriesInChapters(5, 3) },
            { "Read the Poem in 2A", () => HasFlag("cutscene:2:end_s1") },
            { "Read the Poem in Awake", () => HasFlag("cutscene:2:end_s1") },
            { "Talk to Theo in Elevator Shaft", () => HasFlag("cutscene:3:09-d") },
            { "Complete Intervention without jumping", null }, // generated
            { "All Berries in Crossing (9)", () => HasCheckpointBerries(1, 1) },
            { "10 Berries in 2A", () => HasNBerriesInChapter(10, 2) },
            { "12 Berries in 2A", () => HasNBerriesInChapter(12, 2) },
            { "Find Letter and PICO-8 in Huge Mess", () => (HasFlag("cutscene:3:11-a") + HasFlag("foundpico")) / 2f },
            { "Read the Letter in Huge Mess", () => HasFlag("cutscene:3:11-a") },
            { "Get a 1-Up in 2 Chapters", () => HasN1upsInChapters(1, 2) },
            { "Grabless Start of 3A", null }, // generated
            { "Get a 1-Up in 2A", () => HasN1upsInChapter(1, 2) },
            { "Reach Old Site in PICO-8", () => HasFlag("pico_oldsite") },
            { "Forsaken City B-Side", () => HasHeart(1, 1) },
            { "All Collectibles in 1A", () => (HasNBerriesInChapter(20, 1, false)*20f + HasHeart(1, 0) + HasCassette(1)) / 22f },
            { "5 Berries in 4 Chapters", () => HasNBerriesInChapters(5, 4) },
            { "Complete Crossing without dashing", null }, // generated
            { "Complete Shrine without dashing", null }, // generated
            { "Grabless Start of 4A", null }, // generated
            { "Get a 1-Up in 4A", () => HasN1upsInChapter(1, 4) },
            { "2 Cassettes", () => HasNCassettes(2) },
            { "20 Berries", () => HasNBerries(20) },
            { "Get 5 Berries in PICO-8", () => HasPicoBerries(5) },
            { "Old Site B-Side", () => HasHeart(2, 1) },
            { "Grabless 1A", null }, // generated
            { "Grabless 2A", null }, // generated
            { "3 Blue Hearts", () => HasNHeartsColor(3, 0) },
            { "Get three 1-Ups", () => HasN1ups(3) },
            { "Get a 1-Up in 5A", () => HasN1upsInChapter(1, 5) },
            { "All Berries in Elevator Shaft (4)", () => HasCheckpointBerries(3, 2) },
            { "2 Winged Berries in 2 Chapters", () => HasNWingBerriesInChapters(2, 2) },
            { "All Berries in Huge Mess (7)", () => HasCheckpointBerries(3, 1) },
            { "Blue and Red Heart in Forsaken City", () => (HasHeart(1, 0) + HasHeart(1, 1)) / 2f },
            { "Blue and Red Heart in Old Site", () => (HasHeart(2, 0) + HasHeart(2, 1)) / 2 },
            { "All Collectibles in 2A", () => (HasNBerriesInChapter(18, 2, false)*18f + HasHeart(2, 0) + HasCassette(2)) / 20f },
            { "Celestial Resort Blue Heart", () => HasHeart(3, 0) },
            { "All Berries in Presidential Suite (3)", () => HasCheckpointBerries(3, 3) },
            { "Mirror Temple Cassette", () => HasCassette(5) },
            { "Huge Mess: Chest -> Books -> Towel", () => HasHugeMessOrder(2, 1, 0) },
            { "Huge Mess: Chest -> Towel -> Books", () => HasHugeMessOrder(2, 0, 1) },
            { "Huge Mess: Towel -> Books -> Chest", () => HasHugeMessOrder(0, 1, 2) },
            { "Huge Mess: Chest \u2193 \n Books \u2191 \n Towel \u2192", () => HasHugeMessOrder(2, 1, 0) },
            { "Huge Mess: Chest \u2193 \n Towel \u2192 \n Books \u2191", () => HasHugeMessOrder(2, 0, 1) },
            { "Huge Mess: Towel \u2192 \n Books \u2191 \n Chest \u2193", () => HasHugeMessOrder(0, 1, 2) },
            { "Huge Mess: Chest \u2193 Books \u2191 Towel \u2192", () => HasHugeMessOrder(2, 1, 0) },
            { "Huge Mess: Chest \u2193 Towel \u2192 Books \u2191", () => HasHugeMessOrder(2, 0, 1) },
            { "Huge Mess: Towel \u2192 Books \u2191 Chest \u2193", () => HasHugeMessOrder(0, 1, 2) },
            { "2 Seeded Berries", () => HasNSeedBerries(2) },
            { "2 optional Theo Cutscenes", () => HasNFlags(2, TheoCutscenes) },
            { "2 optional Theo cutscenes", () => HasNFlags(2, TheoCutscenes) },
            { "Get a 1-Up in 3 Chapters", () => HasN1upsInChapters(1, 3) },
            { "Read Diary in Elevator Shaft", () => HasFlag("cutscene:3:02-c") },
            { "Read the Diary in Elevator Shaft", () => HasFlag("cutscene:3:02-c") },
            { "Grabless Elevator Shaft", null }, // generated
            { "All Berries in Start of 4A (8)", () => HasCheckpointBerries(4, 0) },
            { "Golden Ridge Cassette", () => HasCassette(4) },
            { "Complete 3 A-Sides", () => HasNASides(3) },
            { "All Berries in Old Trail (7)", () => HasCheckpointBerries(4, 2) },
            { "3 Winged Berries", () => HasNWingBerries(3) },
            { "Use 4 Binoculars in B-Sides", () => HasNBinosInBSides(4) },
            { "Use 5 Binoculars in B-Sides", () => HasNBinosInBSides(5) },
            { "5 Berries in 5 Chapters", () => HasNBerriesInChapters(5, 5) },
            { "Grabless Huge Mess", null }, // generated
            { "Huge Mess: Books -> Towel -> Chest", () => HasHugeMessOrder(1, 0, 2) },
            { "Huge Mess: Books -> Chest -> Towel", () => HasHugeMessOrder(1, 2, 0) },
            { "Huge Mess: Towel -> Chest -> Books", () => HasHugeMessOrder(0, 2, 1) },
            { "Huge Mess: Books \u2191 \n Towel \u2192 \n Chest \u2193", () => HasHugeMessOrder(1, 0, 2) },
            { "Huge Mess: Books \u2191 \n Chest \u2193 \n Towel \u2192", () => HasHugeMessOrder(1, 2, 0) },
            { "Huge Mess: Towel \u2192 \n Chest \u2193 \n Books \u2191", () => HasHugeMessOrder(0, 2, 1) },
            { "Huge Mess: Books \u2191 Towel \u2192 Chest \u2193", () => HasHugeMessOrder(1, 0, 2) },
            { "Huge Mess: Books \u2191 Chest \u2193 Towel \u2192", () => HasHugeMessOrder(1, 2, 0) },
            { "Huge Mess: Towel \u2192 Chest \u2193 Books \u2191", () => HasHugeMessOrder(0, 2, 1) },
            { "Jump on 10 Snowballs", () => HasNSnowballs(10) },
            { "Grabless Presidential Suite", null }, // generated
            { "25 Berries", () => HasNBerries(25) },
            { "Golden Ridge Blue Heart", () => HasHeart(4, 0) },
            { "Grabless Cliff Face", null }, // generated
            { "4 Blue Hearts", () => HasNHeartsColor(4, 0) },
            { "Find Theo's Phone in 5A", () => HasFlag("cutscene:5:a-00c") },
            { "Find Theo's Phone in Start of 5A", () => HasFlag("cutscene:5:a-00c") },
            { "Celestial Resort Cassette", () => HasCassette(3) },
            { "Grabless Unraveling", null }, // generated
            { "Get 2 Keys in 5B", () => (HasFlag("key:5:1:b-02:221") + HasFlag("key:5:1:b-02:219")) / 2f },
            { "All Berries in Cliff Face (5)", () => HasCheckpointBerries(4, 3) },
            { "Grabless Search", null }, // generated
            { "4 Winged Berries", () => HasNWingBerries(4) },
            { "5 Winged Berries", () => HasNWingBerries(5) },
            { "Talk to Theo in Search", () => HasFlag("cutscene:5:search") },
            { "4 Cassettes", () => HasNCassettes(4) },
            { "15 Berries in 4A", () => HasNBerriesInChapter(15, 4) },
            { "Use 6 Binoculars in B-Sides", () => HasNBinosInBSides(6) },
            { "Get 10 Berries in PICO-8", () => HasPicoBerries(10) },
            { "Stun Oshiro 10 times", () => HasOshiroBonks(10) },
            { "Stun Oshiro 10 Times", () => HasOshiroBonks(10) },
            { "Golden Ridge B-Side", () => HasHeart(4, 1) },
            { "Winged Golden Berry", () => HasParticularStrawberries(1, "end:4") },
            { "Mirror Temple A-Side", () => HasChapterComplete(5, 0) },
            { "Grabless Depths", null }, // generated
            { "10 Berries in 3A", () => HasNBerriesInChapter(10, 3) },
            { "Complete 2 B-Sides", () => HasNHeartsColor(2, 1) },
            { "Get the Key in Depths", () => HasFlag("key:5:0:b-04:3") },
            { "All Berries in Rescue (1)", () => HasCheckpointBerries(5, 4) },
            { "Mirror Temple Blue Heart", () => HasHeart(5, 0) },
            { "All Berries in Start of 5A (12)", () => HasCheckpointBerries(5, 0) },
            { "All Berries in Start of 3A (11)", () => HasCheckpointBerries(3, 0) },
            { "All Berries in Shrine (9)", () => HasCheckpointBerries(4, 1) },
            { "Hit a Kevin block from all 4 sides", () => HasFlag("kevin") },
            { "All Berries in Unraveling (1)", () => HasCheckpointBerries(5, 2) },
            { "2 Blue and 2 Red Hearts", () => (HasNHeartsColor(2, 0) + HasNHeartsColor(2, 1)) / 2 },
            { "Get the Orb in PICO-8", () => HasFlag("pico_orb") },
            { "Get 1 Key in Power Source", () => HasNFlags(1, KeysFW) },
            { "Reach Library (3B Checkpoint)", () => HasCheckpoint(3, 1, 2) },
            { "All Berries in Into the Core (1)", () => HasCheckpointBerries(9, 1) },
            { "Reflection Cutscene in Hollows", () => HasFlag("cutscene:6:04d") },
            { "Grabless Lake", null }, // generated
            { "35 Berries", () => HasNBerries(35) },
            { "40 Berries", () => HasNBerries(40) },
            { "Complete PICO-8", () => HasFlag("pico_complete") },
            { "3 optional Theo Cutscenes", () => HasNFlags(3, TheoCutscenes) },
            { "3 optional Theo cutscenes", () => HasNFlags(3, TheoCutscenes) },
            { "Kill a Seeker", () => HasSeekerKills(1) },
            { "Use 1 Binocular in 4 Chapters", () => HasNBinosInChapters(1, 4) },
            { "Only top route in Hollows", () => HasFlag("room:hollows:top") },
            { "Get 1 Key in Search", () => HasNFlags(1, KeysSearch) },
            { "Reflection Cassette", () => HasCassette(6) },
            { "Reflection Blue Heart", () => HasHeart(6, 0) },
            { "3 Seeded Berries", () => HasNSeedBerries(3) },
            { "Complete 2 A-Sides and 2 B-Sides", () => (HasNASides(2) + HasNHeartsColor(2, 1)) / 2f },
            { "5 Cassettes", () => HasNCassettes(5) },
            { "Stun Seekers 15 times", () => HasSeekerStuns(15) },
            { "Stun Seekers 15 Times", () => HasSeekerStuns(15) },
            { "Get 2 Keys in Power Source", () => HasNFlags(2, KeysFW) },
            { "Use 5 Binoculars in Farewell", () => HasNBinosInChapter(5, 10, 0) },
            { "All Berries in Depths (11)", () => HasCheckpointBerries(5, 1) },
            { "All Berries in Search (6)", () => HasCheckpointBerries(5, 3) },
            { "Mirror Temple B-Side", () => HasHeart(5, 1) },
            { "5 Hearts", () => HasNHearts(5) },
            { "20 Berries in 5A", () => HasNBerriesInChapter(20, 5) },
            { "Use 7 Binoculars", () => HasNBinos(7) },
            { "Use 8 Binoculars", () => HasNBinos(8) },
            { "Use 2 Binoculars in 3 Chapters", () => HasNBinosInChapters(2, 3) },
            { "Get 15 Berries in PICO-8", () => HasPicoBerries(15) },
            { "Celestial Resort B-Side", () => HasHeart(3, 1) },
            { "Only bottom route in Hollows", () => HasFlag("room:hollows:bottom") },
            { "Get 2 Keys in Search", () => HasNFlags(2, KeysSearch) },
            { "Get 3 Keys in Search", () => HasNFlags(3, KeysSearch) },
            { "6 Winged Berries", () => HasNWingBerries(6) },
            { "7 Winged Berries", () => HasNWingBerries(7) },
            { "Kill 2 different Seekers", () => HasSeekerKills(2) },
            { "Kill 2 Different Seekers", () => HasSeekerKills(2) },
            { "Get 3 Keys in Power Source", () => HasNFlags(3, KeysFW) },
            { "Get 4 Keys in Power Source", () => HasNFlags(4, KeysFW) },
            { "Use 1 Binocular in 5 Chapters", () => HasNBinosInChapters(1, 5) },
            { "10 Berries in 3 Chapters", () => HasNBerriesInChapters(10, 3) },
            { "Switch to Ice on the right of Into the Core", () => HasFlag("first_ice") },
            { "5 Blue Hearts", () => HasNHeartsColor(5, 0) },
            { "Complete 3 B-Sides", () => HasNHeartsColor(3, 1) },
            { "3 Blue and 3 Red Hearts", () => (HasNHeartsColor(3, 0) + HasNHeartsColor(3, 1)) / 2f },
            { "Complete 2 Chapters Grabless", () => HasChaptersVariant(2, BingoVariant.NoGrab) },
            { "Stun Seekers 20 times", () => HasSeekerStuns(20) },
            { "Stun Seekers 20 Times", () => HasSeekerStuns(20) },
            { "Kill 3 different Seekers", () => HasSeekerKills(3) },
            { "Kill 3 Different Seekers", () => HasSeekerKills(3) },
            { "Blue and Red Heart in Golden Ridge", () => (HasHeart(4, 0) + HasHeart(4, 1)) / 2f },
            { "Grabless Hollows", null }, // generated
            { "15 Berries in 2 Chapters", () => HasNBerriesInChapters(15, 2) },
            { "15 Berries in 3A", () => HasNBerriesInChapter(15, 3) },
            { "5 Berries in 8A", () => HasNBerriesInChapter(15, 9) },
            { "45 Berries", () => HasNBerries(45) },
            { "50 Berries", () => HasNBerries(50) },
            { "Reach Rock Bottom (6A/6B Checkpoint)", () => Math.Max(HasCheckpoint(6, 0, 4), HasCheckpoint(6, 1, 2)) },
            { "Use 2 Binoculars in 4 Chapters", () => HasNBinosInChapters(2, 4) },
            { "Blue and Red Heart in Mirror Temple", () => (HasHeart(5, 0) + HasHeart(5, 1)) / 2f },
            { "All Berries in Heart of the Mountain (1)", () => HasCheckpointBerries(9, 3) },
            { "Use all Binoculars in 500M (3)", () => HasParticularBinos(7, 0, "b-01", "b-02", "b-02b") },
            { "Use All Binoculars in 500M (3)", () => HasParticularBinos(7, 0, "b-01", "b-02", "b-02b") },
            { "All Berries in 0M (4)", () => HasCheckpointBerries(7, 0) },
            { "Reflection A-Side", () => HasChapterComplete(6, 0) },
            { "All Collectibles in 4A", () => (HasNBerriesInChapter(29, 4, false)*29f + HasCassette(4) + HasHeart(4, 0)) / 31f },
            { "8 Winged Berries", () => HasNWingBerries(8) },
            { "9 Winged Berries", () => HasNWingBerries(9) },
            { "Complete 3 A-Sides and 3 B-Sides", () => (HasNASides(3) + HasNHeartsColor(3, 1)) / 2f },
            { "3 Gems in The Summit", () => HasNSummitGems(3) },
            { "0M and 500M Gems", () => HasSummitGems(0, 1) },
            { "Grabless 3A", null }, // generated
            { "10 Berries in 4 Chapters", () => HasNBerriesInChapters(10, 4) },
            { "All Collectibles in 3A", () => (HasNBerriesInChapter(25, 3, false)*25f + HasCassette(3) + HasHeart(3, 0)) / 27f },
            { "Grabless Rock Bottom", null }, // generated
            { "Easteregg room in Reflection", () => HasFlag("room:easteregg") },
            { "Easteregg Room in Reflection", () => HasFlag("room:easteregg") },
            { "4 Seeded Berries", () => HasNSeedBerries(4) },
            { "6 Hearts", () => HasNHearts(6) },
            { "7 Hearts", () => HasNHearts(7) },
            { "20 Berries in 7A", () => HasNBerriesInChapter(20, 7) },
            { "Use 9 Binoculars", () => HasNBinos(9) },
            { "Use 10 Binoculars", () => HasNBinos(10) },
            { "Grabless 5A", null }, // generated
            { "All Berries in 500M (6)", () => HasCheckpointBerries(7, 1) },
            { "All Berries in 1000M (6)", () => HasCheckpointBerries(7, 2) },
            { "All 4 optional Theo Cutscenes", () => HasNFlags(4, TheoCutscenes) },
            { "All 4 optional Theo cutscenes", () => HasNFlags(4, TheoCutscenes) },
            { "Complete 4 A-Sides", () => HasNASides(4) },
            { "Complete 5 A-Sides", () => HasNASides(5) },
            { "The Summit Cassette", () => HasCassette(7) },
            { "65 Berries", () => HasNBerries(65) },
            { "75 Berries", () => HasNBerries(75) },
            { "4 Gems in The Summit", () => HasNSummitGems(4) },
            { "1000M and 1500M Gems", () => HasSummitGems(2, 3) },
            { "Blue and Red Heart in Celestial Resort", () => (HasHeart(3, 0) + HasHeart(3, 1)) / 2f },
            { "10 Berries in 5 Chapters", () => HasNBerriesInChapters(10, 5) },
            { "Use 5 Binoculars in The Summit", () => HasNBinosInChapter(5, 7, 0) },
            { "Use all Binoculars in 1000M (4)", () => HasParticularBinos(7, 0, "c-03b", "c-05", "c-06c", "c-07b") },
            { "Use All Binoculars in 1000M (4)", () => HasParticularBinos(7, 0, "c-03b", "c-05", "c-06c", "c-07b") },
            { "Complete 4 B-Sides", () => HasNHeartsColor(4, 1) },
            { "Reflection B-Side", () => HasHeart(6, 1) },
            { "Reach the Intro Car in Remembered", () => HasFlag("remembered_intro_car") },
            { "Reach an Intro Car in Farewell", () => HasFlag("fw_intro_car") },
            { "Reach the Orb in Heart of the Mountain", () => HasFlag("fw_intro_car") },
            { "5 Gems in the Summit", () => HasNSummitGems(5) },
            { "5 Gems in The Summit", () => HasNSummitGems(5) },
            { "2000M and 2500M Gems", () => HasSummitGems(4, 5) },
            { "15 Berries in 3 Chapters", () => HasNBerriesInChapters(15, 3) },
            { "Grabless 6A", null }, // generated
            { "All Berries in 1500M (8)", () => HasCheckpointBerries(7, 3) },
            { "All Berries in 2000M (8)", () => HasCheckpointBerries(7, 4) },
            { "Get 5 Keys in Power Source", () => HasNFlags(5, KeysFW) },
            { "35 Berries in 7A", () => HasNBerriesInChapter(35, 7) },
            { "Blue and Red Heart in Reflection", () => (HasHeart(6, 0) + HasHeart(6, 1)) / 2f },
            { "All Flags in 3000M (7A/7B Checkpoint)", () => HasFlag("all_summit_flags") },
            { "All Collectibles in 5A", () => (HasNBerriesInChapter(31, 5, false)*31f + HasCassette(5) + HasHeart(5, 0)) / 33f },
            { "All Berries in 2500M (8)", () => HasCheckpointBerries(7, 5) },
            { "All Berries in 3000M (7)", () => HasCheckpointBerries(7, 6) },
            { "All Berries in Hot and Cold (3)", () => HasCheckpointBerries(9, 2) },
            { "Use 2 Binoculars in 5 Chapters", () => HasNBinosInChapters(2, 5) },
            { "Core Blue Heart", () => HasHeart(9, 0) },
            { "Visit the Bird's Nest in Epilogue", () => HasFlag("room:birdnest") },
            { "The Summit B-Side", () => HasHeart(7, 1) },
            { "100 Berries", () => HasNBerries(100) },
            { "Complete 5 B-Sides", () => HasNHeartsColor(5, 1) },
            { "15 Berries in 4 Chapters", () => HasNBerriesInChapters(15, 4) },
            { "Reach Event Horizon (9 Checkpoint)", () => HasCheckpoint(10, 0, 4) },
            { "Reach Event Horizon (FW Checkpoint)", () => HasCheckpoint(10, 0, 4) },
            { "Reach 2000m (7B Checkpoint)", () => HasCheckpoint(7, 0, 4) },
            { "Reach 2000M (7B Checkpoint)", () => HasCheckpoint(7, 0, 4) },
            { "All Collectibles in 8A", () => (HasNBerriesInChapter(5, 9, false)*5f + HasCassette(9) + HasHeart(9, 0)) / 7f },
            { "The Summit Blue Heart", () => HasHeart(7, 0) },
            { "2 Keys in 2 Chapters", () => HasNKeysInChapters(2, 2) },
            { "5 Keys in 2 Chapters", () => HasNKeysInChapters(5, 2) },
            { "5 Keys", () => HasNFlags(5, KeysAll) },
            { "8 Keys", () => HasNFlags(8, KeysAll) },
            { "12 Keys", () => HasNFlags(12, KeysAll) },
            { "2 Winged Berries", () => HasNWingBerries(2) },
            { "Take hidden path before Cliff Face", () => HasFlag("room:oldtrailsecret") },
            { "Complete Awake without jumping", null }, // generated
            { "2 Hearts", () => HasNHearts(2) },
            { "Complete 1 B-Side", () => HasNHeartsColor(1, 1) },
            { "Use 3 Binoculars in B-Sides", () => HasNBinosInBSides(3) },
            { "2 Hearts and 2 Cassettes", () => (HasNHearts(2) + HasNCassettes(2)) / 2 },
            { "1 Blue and 1 Red Heart", () => (HasNHeartsColor(1, 0) + HasNHeartsColor(1, 1)) / 2 },
            { "Use all Binoculars in 4A (3)", () => HasNBinosInChapter(3, 4, 0) },
            { "Use All Binoculars in 4A (3)", () => HasNBinosInChapter(3, 4, 0) },
            { "Jump on 15 Snowballs", () => HasNSnowballs(15) },
            { "3 Cassettes", () => HasNCassettes(3) },
            { "30 Berries", () => HasNBerries(30) },
            { "Use 2 Binoculars in 2 Chapters", () => HasNBinosInChapters(2, 2) },
            { "Use 5 Binoculars", () => HasNBinos(5) },
            { "Use 6 Binoculars", () => HasNBinos(6) },
            { "Complete Start of 5A without jumping", null }, // generated
            { "3 Hearts", () => HasNHearts(3) },
            { "4 Hearts", () => HasNHearts(4) },
            { "Stun Oshiro 15 times", () => HasOshiroBonks(15) },
            { "Stun Oshiro 15 Times", () => HasOshiroBonks(15) },
            { "Use 1 Binocular in 3 Chapters", () => HasNBinosInChapters(1, 3) },
            { "Grabless Rescue", null }, // generated
            { "3 Hearts and 3 Cassettes", () => (HasNHearts(3) + HasNCassettes(3)) / 2 },
            { "4 Hearts and 4 Cassettes", () => (HasNHearts(4) + HasNCassettes(4)) / 2 },
            { "Stun Seekers 10 times", () => HasSeekerStuns(10) },
            { "Stun Seekers 10 Times", () => HasSeekerStuns(10) },
            { "15 Berries in 5A", () => HasNBerriesInChapter(15, 5) },
            { "Don't skip final 4A Cutscene", () => HasFlag("cutscene:4:d-10") },
            { "Use 3 Binoculars in 2 Chapters", () => HasNBinosInChapters(3, 2) },
            { "Reach Rock Bottom (6B Checkpoint)", () => HasCheckpoint(6, 1, 2) },
            { "20 Berries in 4A", () => HasNBerriesInChapter(20, 4) },
            { "Complete Resolution without jumping", null }, // generated
            { "Empty Space", () => HasFlag("empty_space") },
            { "Clear Core", () => HasHeart(9, 0) },

            { "Triple 1-Up", () => Has1upCombo(3) },
            { "Crossing Dashless", null }, // generated
            { "Jumpless Dashless Awake", null }, // generated
            { "Complete Forsaken City with Low Friction", null }, // generated
            { "2 Chapters Grabless", () => HasChaptersVariant(2, BingoVariant.NoGrab) },
            { "Two Blue and Two Red Hearts", () => (HasNHeartsColor(2, 0) + HasNHeartsColor(2, 1)) / 2f },
            { "3 B-Sides", () => HasNHeartsColor(3, 1) },
            { "Complete Forsaken City with 70% Speed", null }, // generated
            { "Complete Forsaken City with 160% Speed", null }, // generated
            { "Complete Old Site with Low Friction", null }, // generated
            { "Full Clear 1A", () => HasChapterClear(1) },
            { "Quintuple 1-Up", () => Has1upCombo(5) },
            { "Winged Golden", () => HasParticularStrawberries(1, "end:4") },
            { "Complete 2 Chapters Mirrored", () => HasChaptersVariant(2, BingoVariant.Mirrored) },
            { "Complete Pico-8", () => HasFlag("pico_complete") },
            { "Get a 1-Up in 4 Chapters", () => HasN1upsInChapters(1, 4) },
            { "Complete Intervention without Jumping", null }, // generated
            { "Complete Old Trail with Low Friction", null }, // generated
            { "100% Pico-8", () => (HasFlag("pico_complete") + HasPicoBerries(18)) / 2f },
            { "Complete Presidential Suite with Low Friction", null }, // generated
            { "Bop Oshiro 10 Times in Two Chapters", () => HasOshiroBonksInChapters(10, 2) },
            { "All Berries in Start and Depths of 5A (23)", () => (HasCheckpointBerries(5, 0) + HasCheckpointBerries(5, 1)) / 2f },
            { "Invisible Huge Mess", null }, // generated
            { "Complete Elevator Shaft with Low Friction", null }, // generated
            { "Complete 4 Chapters Mirrored", () => HasChaptersVariant(4, BingoVariant.Mirrored) },
            { "Get 4 Hearts and 4 Cassettes", () => (HasNHearts(4) + HasNCassettes(4)) / 2f },
            { "Invisible Forsaken City", null }, // generated
            { "4 Chapters Hiccups", () => HasChaptersVariant(4, BingoVariant.Hiccups) },
            { "Get a 1-Up in 3A", () => HasN1upsInChapter(1, 3) },
            { "Bop Seekers 10 Times in 2 Chapters", () => HasSeekerStunsInChapters(10, 2) },
            { "Complete 3A Start with Low Friction", null }, // generated
            { "Invisible Unravelled", null }, // generated
            { "Invisible Cliff Face", null }, // generated
            { "Complete Start of Mirror Temple with Low Friction", null }, // generated
            { "Invisible Start of Celestial Resort", null }, // generated
            { "Invisible Old Site", null }, // generated
            { "Grabless Huge Mess with the Heart", () => HasFlag("grabless_huge_mess_with_heart") },
            { "Complete 3 Chapters with Low Friction", () => HasChaptersVariant(3, BingoVariant.LowFriction) },
            { "Full Clear 8A", () => HasChapterClear(9) },
            { "Grabless Celestial Resort", null }, // generated
            { "Kill 5 Different Seekers", () => HasSeekerKills(5) },
            { "Complete Mirror Temple with Low Friction", null }, // generated
            { "Kill 3 Different Seekers in Two Chapters", () => HasSeekerKillsInChapters(3, 2) },
            { "Get 15 Berries in 4 Chapters", () => HasNBerriesInChapters(15, 4) },
            { "15 Berries in 5 Chapters", () => HasNBerriesInChapters(15, 4) },
            { "Intro Car in Remembered", () => HasFlag("remembered_intro_car") },
            { "Reach an Intro Car in Remembered", () => HasFlag("remembered_intro_car") },
            { "1-Up in 7A", () => HasN1upsInChapter(1, 7) },
            { "Jumpless Reflection (Checkpoint)", null }, // generated
            { "Jumpless Resolution", null }, // generated
            { "Grabless Mirror Temple", null }, // generated
            { "Invisible Mirror Temple", null }, // generated
            { "Grabless Repreive", null }, // generated
            { "5 Berries in 7 Chapters", () => HasN1upsInChapters(5, 7) },
            { "Invisible Presidential Suite", null }, // generated
            { "Bounce on 10 Snowballs in 2 Chapters", () => HasSnowballsInChapters(10, 2) },
            { "3000M Grabless", null }, // generated
            { "Complete 2 Chapters Invisible", () => HasChaptersVariant(2, BingoVariant.Invisible) },
            { "Invisible 1500M", null }, // generated
            { "Invisible 2500M", null }, // generated
            { "Every Different Seeker Kill (12)", () => HasSeekerKills(12) },
            { "Complete 5 Chapters Grabless", () => HasChaptersVariant(5, BingoVariant.NoGrab) },
            { "Use 4 Binoculars in 8 Chapters", () => HasNBinosInChapters(4, 8) },
            { "Full Clear Three Chapters", () => HasChaptersClear(3) },
            { "Invisible 3000M", null }, // generated
            { "69 Berries", () => HasNBerries(69) },
            { "Blue Heart in The Summit", () => HasHeart(7, 0) },
            { "Find the Bird's Nest in Epilogue", () => HasFlag("room:birdnest") },
            { "20 Berries in 3 Chapters", () => HasNBerriesInChapters(20, 3) },
            { "Invisible Power Source", null }, // generated
            { "Complete 5 Chapters with Low Friction", () => HasChaptersVariant(5, BingoVariant.LowFriction) },
            { "Complete 3000M with no Badeline Orbs", () => HasFlag("orbless_3000m") },
            { "20 Berries in 4 Chapters", () => HasNBerriesInChapters(20, 4) },
            { "Use 2 Binoculars in 8 Chapters", () => HasNBinosInChapters(2, 8) },
            { "All Winged Berries (11)", () => HasNWingBerries(11) },
            { "Complete 2B Grabless", null }, // generated
            { "Complete Rooftop Jumpless", null }, // generated
            { "Complete 6 Chapters Grabless", () => HasChaptersVariant(6, BingoVariant.NoGrab) },
            { "Reach Event Horizon", () => HasCheckpoint(10, 0, 4) },
            { "Use All Binoculars in A-Sides (14)", () => HasNBinosInASides(14) },
            { "Complete 6 Chapters Invisible", () => HasChaptersVariant(6, BingoVariant.Invisible) },
            { "Use All Binoculars in B-Sides (13)", () => HasNBinosInBSides(13) },
            { "Use 20 Binoculars in Farewell", () => HasNBinosInChapter(20, 10, 0) },
            { "10 Hearts", () => HasNHearts(10) },
            { "6 Hearts and 6 Cassettes", () => (HasNHearts(6) + HasNCassettes(6)) / 2f },
            { "Full Clear 7A", () => HasChapterClear(7) },
            { "8 Cassettes", () => HasNCassettes(8) },
            { "Blue and Red Heart in The Summit", () => (HasHeart(7, 0) + HasHeart(7, 1)) / 2f },
            { "Complete Remembered with Low Friction", null }, // generated
            { "25 Berries in 4 Chapters", () => HasNBerriesInChapters(25, 4) },
            { "Determination Demo Room", () => HasFlag("room:determinationdemo") },
            { "20 Berries in 5 Chapters", () => HasNBerriesInChapters(20, 5) },
            { "Complete 10 Chapters", () => HasNChapters(10) },
            { "4 A-Sides and 4 B-Sides", () => (HasNASides(4) + HasNHeartsColor(4, 1)) / 2f },
            { "12 Hearts", () => HasNHearts(12) },
            { "Unlock Reconciliation", () => HasCheckpoint(10, 0, 7) },
            { "Five Full Clears", () => HasChaptersClear(5) },
            { "Complete 6 B-Sides", () => HasNHeartsColor(6, 1) },
            { "Grabless 6B", null }, // generated
            { "Moon Berry", () => HasParticularStrawberries(10, "j-19:9") },
            { "125 Berries", () => HasNBerries(125) },
            { "All Blue Hearts", () => HasNHeartsColor(8, 0) },
            { "7 Red Hearts", () => HasNHeartsColor(7, 1) },
            { "Farewell", () => HasChapterComplete(10, 0) },
        };

        public static Dictionary<string, List<Tuple<int, int, int, BingoVariant>>> ObjectiveVariants = new Dictionary<string, List<Tuple<int, int, int, BingoVariant>>> {
            {"Complete 1A Start without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 0, BingoVariant.NoDash)}},
            {"Complete 1A Start without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 0, BingoVariant.NoJump)}},
            {"Complete Crossing without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 1, BingoVariant.NoDash)}},
            {"Complete Chasm without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 2, BingoVariant.NoDash)}},
            {"Crossing Dashless", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 1, BingoVariant.NoDash)}},
            {"Dashless Start of 1A", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 0, BingoVariant.NoDash)}},
            {"Dashless Chasm", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 2, BingoVariant.NoDash)}},
            {"Dashless Crossing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 2, BingoVariant.NoDash)}},
            {"Complete Intervention without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(2, 0, 1, BingoVariant.NoJump)}},
            {"Complete Intervention without Jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(2, 0, 1, BingoVariant.NoJump)}},
            {"Complete Awake without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(2, 0, 2, BingoVariant.NoDash)}},
            {"Complete Awake without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(2, 0, 2, BingoVariant.NoJump)}},
            {"Jumpless Dashless Awake", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(2, 0, 2, BingoVariant.NoJumpNoDash)}},
            {"Grabless Start of 3A", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 0, BingoVariant.NoGrab)}},
            {"Invisible Start of Celestial Resort", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 0, BingoVariant.Invisible)}},
            {"Complete 3A Start with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 0, BingoVariant.LowFriction)}},
            {"Grabless Huge Mess", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 1, BingoVariant.NoGrab)}},
            {"Invisible Huge Mess", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 1, BingoVariant.Invisible)}},
            {"Complete Elevator Shaft with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 2, BingoVariant.LowFriction)}},
            {"Grabless Elevator Shaft", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 2, BingoVariant.NoGrab)}},
            {"Grabless Presidential Suite", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 3, BingoVariant.NoGrab)}},
            {"Invisible Presidential Suite", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 3, BingoVariant.Invisible)}},
            {"Complete Presidential Suite with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 3, BingoVariant.LowFriction)}},
            {"Complete Rooftop Jumpless", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 1, 3, BingoVariant.NoJump)}},
            {"Grabless Start of 4A", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(4, 0, 0, BingoVariant.NoGrab)}},
            {"Complete Shrine without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(4, 0, 1, BingoVariant.NoDash)}},
            {"Dashless Shrine", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(4, 0, 1, BingoVariant.NoDash)}},
            {"Complete Old Trail with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(4, 0, 2, BingoVariant.NoJump)}},
            {"Grabless Cliff Face", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(4, 0, 3, BingoVariant.NoGrab)}},
            {"Invisible Cliff Face", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(4, 0, 3, BingoVariant.Invisible)}},
            {"Complete Start of 5A without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 0, BingoVariant.NoJump)}},
            {"Complete Start of Mirror Temple with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 0, BingoVariant.LowFriction)}},
            {"Grabless Depths", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 1, BingoVariant.NoGrab)}},
            {"Grabless Unraveling", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 2, BingoVariant.NoGrab)}},
            {"Invisible Unravelled", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 2, BingoVariant.Invisible)}},
            {"Grabless Search", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 3, BingoVariant.NoGrab)}},
            {"Grabless Rescue", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 4, BingoVariant.NoGrab)}},
            {"Grabless Lake", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 1, BingoVariant.NoGrab)}},
            {"Grabless Hollows", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 2, BingoVariant.NoGrab)}},
            {"Jumpless Reflection (Checkpoint)", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 3, BingoVariant.NoJump)}},
            {"Grabless Rock Bottom", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 4, BingoVariant.NoGrab)}},
            {"Grabless Rock Bottom (6A/6B Checkpoint)", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 4, BingoVariant.NoGrab)}},
            {"Complete Resolution without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 5, BingoVariant.NoJump)}},
            {"Jumpless Resolution", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 5, BingoVariant.NoJump)}},
            {"Grabless Repreive", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 1, 3, BingoVariant.NoGrab)}},
            {"Invisible 1500M", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(7, 0, 3, BingoVariant.Invisible)}},
            {"Invisible 2500M", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(7, 0, 5, BingoVariant.Invisible)}},
            {"Invisible 3000M", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(7, 0, 6, BingoVariant.Invisible)}},
            {"3000M Grabless", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(7, 0, 6, BingoVariant.NoGrab)}},
            {"Grabless Power Source", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(10, 0, 2, BingoVariant.NoGrab)}},
            {"Invisible Power Source", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(10, 0, 2, BingoVariant.Invisible)}},
            {"Complete Remembered with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(10, 0, 3, BingoVariant.LowFriction)}},
            {
                "Grabless 1A", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(1, 0, 0, BingoVariant.NoGrab),
                    Tuple.Create(1, 0, 1, BingoVariant.NoGrab),
                    Tuple.Create(1, 0, 2, BingoVariant.NoGrab),
                }
            }, {
                "Complete Forsaken City with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(1, 0, 0, BingoVariant.LowFriction),
                    Tuple.Create(1, 0, 1, BingoVariant.LowFriction),
                    Tuple.Create(1, 0, 2, BingoVariant.LowFriction),
                }
            }, {
                "Invisible Forsaken City", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(1, 0, 0, BingoVariant.Invisible),
                    Tuple.Create(1, 0, 1, BingoVariant.Invisible),
                    Tuple.Create(1, 0, 2, BingoVariant.Invisible),
                }
            }, {
                "Complete Forsaken City with 70% Speed", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(1, 0, 0, BingoVariant.Speed70),
                    Tuple.Create(1, 0, 1, BingoVariant.Speed70),
                    Tuple.Create(1, 0, 2, BingoVariant.Speed70),
                }
            }, {
                "Complete Forsaken City with 160% Speed", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(1, 0, 0, BingoVariant.Speed160),
                    Tuple.Create(1, 0, 1, BingoVariant.Speed160),
                    Tuple.Create(1, 0, 2, BingoVariant.Speed160),
                }
            }, {
                "Grabless 2A", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(2, 0, 0, BingoVariant.NoGrab),
                    Tuple.Create(2, 0, 1, BingoVariant.NoGrab),
                    Tuple.Create(2, 0, 2, BingoVariant.NoGrab),
                }
            }, {
                "Complete 2B Grabless", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(2, 1, 0, BingoVariant.NoGrab),
                    Tuple.Create(2, 1, 1, BingoVariant.NoGrab),
                    Tuple.Create(2, 1, 2, BingoVariant.NoGrab),
                }
            }, {
                "Invisible Old Site", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(2, 0, 0, BingoVariant.Invisible),
                    Tuple.Create(2, 0, 1, BingoVariant.Invisible),
                    Tuple.Create(2, 0, 2, BingoVariant.Invisible),
                }
            }, {
                "Complete Old Site with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(2, 0, 0, BingoVariant.LowFriction),
                    Tuple.Create(2, 0, 1, BingoVariant.LowFriction),
                    Tuple.Create(2, 0, 2, BingoVariant.LowFriction),
                }
            }, {
                "Grabless 3A", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(3, 0, 0, BingoVariant.NoGrab),
                    Tuple.Create(3, 0, 1, BingoVariant.NoGrab),
                    Tuple.Create(3, 0, 2, BingoVariant.NoGrab),
                    Tuple.Create(3, 0, 3, BingoVariant.NoGrab),
                }
            }, {
                "Grabless Celestial Resort", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(3, 0, 0, BingoVariant.NoGrab),
                    Tuple.Create(3, 0, 1, BingoVariant.NoGrab),
                    Tuple.Create(3, 0, 2, BingoVariant.NoGrab),
                    Tuple.Create(3, 0, 3, BingoVariant.NoGrab),
                }
            }, {
                "Grabless 5A", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(5, 0, 0, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 1, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 2, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 3, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 4, BingoVariant.NoGrab),
                }
            }, {
                "Grabless Mirror Temple", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(5, 0, 0, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 1, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 2, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 3, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 4, BingoVariant.NoGrab),
                }
            }, {
                "Invisible Mirror Temple", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(5, 0, 0, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 1, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 2, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 3, BingoVariant.NoGrab),
                    Tuple.Create(5, 0, 4, BingoVariant.NoGrab),
                }
            }, {
                "Complete Mirror Temple with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(5, 0, 0, BingoVariant.LowFriction),
                    Tuple.Create(5, 0, 1, BingoVariant.LowFriction),
                    Tuple.Create(5, 0, 2, BingoVariant.LowFriction),
                    Tuple.Create(5, 0, 3, BingoVariant.LowFriction),
                    Tuple.Create(5, 0, 4, BingoVariant.LowFriction),
                }
            }, {
                "Grabless 6A", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(6, 0, 0, BingoVariant.NoGrab),
                    Tuple.Create(6, 0, 1, BingoVariant.NoGrab),
                    Tuple.Create(6, 0, 2, BingoVariant.NoGrab),
                    Tuple.Create(6, 0, 3, BingoVariant.NoGrab),
                    Tuple.Create(6, 0, 4, BingoVariant.NoGrab),
                    Tuple.Create(6, 0, 5, BingoVariant.NoGrab),
                }
            }, {
                "Grabless 6B", new List<Tuple<int, int, int, BingoVariant>> {
                    Tuple.Create(6, 1, 0, BingoVariant.NoGrab),
                    Tuple.Create(6, 1, 1, BingoVariant.NoGrab),
                    Tuple.Create(6, 1, 2, BingoVariant.NoGrab),
                    Tuple.Create(6, 1, 3, BingoVariant.NoGrab),
                }
            },
            {"2 Chapters Grabless", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.NoGrab)}},
            {"Complete 2 Chapters Grabless", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.NoGrab)}},
            {"Complete 5 Chapters Grabless", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.NoGrab)}},
            {"Complete 6 Chapters Grabless", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.NoGrab)}},
            {"Complete 2 Chapters Mirrored", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.Mirrored)}},
            {"Complete 4 Chapters Mirrored", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.Mirrored)}},
            {"Complete 2 Chapters Invisible", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.Invisible)}},
            {"4 Chapters Hiccups", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.Hiccups)}},
            {"Complete 3 Chapters with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.LowFriction)}},
            {"Complete 5 Chapters with Low Friction", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.LowFriction)}},
        };

        static BingoMonitor() {
            foreach (var kv in ObjectiveVariants) {
                if (Objectives.TryGetValue(kv.Key, out var maybenull) && maybenull != null) {
                    continue;
                }

                Func<float> close(List<Tuple<int, int, int, BingoVariant>> reqs) {
                    return () => {
                        if (SaveData.Instance == null) {
                            return 0f;
                        }

                        return reqs.Count(req => BingoClient.Instance.ModSaveData.HasCheckpointVariant(req.Item1, req.Item2, req.Item3, req.Item4)) / (float)reqs.Count;
                    };
                }
                Objectives[kv.Key] = close(kv.Value);
            }
        }

        public static float ObjectiveProgress(string text) {
            if (SaveData.Instance == null || string.IsNullOrEmpty(SaveData.Instance.TheoSisterName)) {
                return 0f;
            }
            if (!Objectives.TryGetValue(text, out var maybenull) || maybenull == null) {
                return 0f;
            }

            return maybenull();
        }

        [Command("test_bingo_json", "Test if the bingo json in the clipboard has all its objectives handled by the monitor")]
        public static void TestBingoJson() {
            try {
                foreach (var tier in JArray.Parse(TextInput.GetClipboardText())) {
                    if (tier is JArray tierArray) {
                        foreach (var objective in tierArray) {
                            if (objective is JObject objectiveObj) {
                                var name = objective.Value<string>("name");
                                if (name != null) {
                                    if (!BingoMonitor.Objectives.ContainsKey(name)) {
                                        Engine.Commands.Log($"Fail: Objective {name} is unmonitored");
                                    }
                                } else {
                                    Engine.Commands.Log("Fail: Objective must have name");
                                }
                            } else {
                                Engine.Commands.Log("Fail: Objective must be object");
                            }
                        }
                    } else {
                        Engine.Commands.Log("Fail: Tier must be array");
                    }
                }
            } catch (JsonException) {
                Engine.Commands.Log("Fail: Could not parse JSON");
            }
        }

        #endregion

        #region checkers
        private static float Has1upCombo(int i) {
            return BingoClient.Instance.ModSaveData.MaxOneUpCombo - 4 >= i ? 1f : 0f;
        }

        private static float HasParticularBinos(int chapter, int mode, params string[] levels) {
            return levels.Count(lvl => BinocularsList.Any(bino => {
                var point = new Point((int) bino.pos.X, (int) bino.pos.Y);
                return bino.areaID == chapter && bino.areaMode == mode && AreaData.Areas[chapter].Mode[mode].MapData.Get(lvl).Bounds.Contains(point);
            })) / (float) levels.Length;
        }

        private static float HasNFlags(int n, params string[] flags) {
            return Math.Min(1f, flags.Count(flag => BingoClient.Instance.ModSaveData.FileFlags.Contains(flag)) / (float) n);
        }

        private static float HasChapterVariant(int chapter, int mode, BingoVariant variant) {
            var missed = false;
            var total = CountCheckpoints(new AreaKey(chapter, (AreaMode) mode));
            for (var ch = 0; ch < total; ch++) {
                if (!BingoClient.Instance.ModSaveData.HasCheckpointVariant(chapter, mode, ch, variant)) {
                    missed = true;
                    break;
                }
            }

            return missed ? 0f : 1f;
        }

        private static float HasChaptersVariant(int chapters, BingoVariant variant) {
            return HasInNChapters((chapter, mode) => HasChapterVariant(chapter, mode, variant), chapters);
        }

        private static float HasFlag(string flag) {
            return BingoClient.Instance.ModSaveData.FileFlags.Contains(flag) ? 1f : 0f;
        }

        private static float HasPicoBerries(int n) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.PicoBerries / (float) n);
        }

        private static float HasHugeMessOrder(int p0, int p1, int p2) {
            return BingoClient.Instance.ModSaveData.HasHugeMessOrder(p0, p1, p2) ? 1f : 0f;
        }

        private static float HasSeekerKills(int n) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.SeekerKills.Count / (float) n);
        }

        private static float HasN1upsInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => mode == 0 ? HasN1upsInChapter(n, ch) : 0f, chapters);
        }

        private static float HasN1upsInChapter(int n, int chapter) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.OneUps[chapter] / (float) n);
        }

        private static float HasN1ups(int n) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.OneUps.Sum() / (float) n);
        }

        private static readonly List<Tuple<int, string>> WingedBerryIDList = new List<Tuple<int, string>> {
            Tuple.Create(1, "9c:2"),
            Tuple.Create(1, "3b:2"),
            Tuple.Create(2, "end_3c:13"),
            Tuple.Create(3, "06-a:7"),
            Tuple.Create(3, "13-b:31"),
            Tuple.Create(4, "c-01:26"),
            Tuple.Create(5, "b-21:99"),
            Tuple.Create(7, "b-04:67"),
            Tuple.Create(7, "d-10b:682"),
            Tuple.Create(7, "e-09:398"),
            Tuple.Create(1, "end:4"),
        };
        private static readonly List<Tuple<int, string>> SeedBerryIDList = new List<Tuple<int, string>> {
            Tuple.Create(2, "d1:67"),
            Tuple.Create(4, "a-10:13"),
            Tuple.Create(5, "b-17:10"),
            Tuple.Create(7, "e-12:504"),
        };

        public static List<Binoculars> BinocularsList => BingoModule.SaveData.BinocularsList;

        private static float HasNSeedBerries(int n) {
            return Math.Min(1f, SeedBerryIDList.Count(id => SaveData.Instance.Areas[id.Item1].Modes[0].Strawberries.Contains(new EntityID {Key = id.Item2})) / (float) n);
        }

        private static float HasNWingBerries(int n) {
            return Math.Min(1f, WingedBerryIDList.Count(id => SaveData.Instance.Areas[id.Item1].Modes[0].Strawberries.Contains(new EntityID {Key = id.Item2})) / (float) n);
        }

        private static float HasNWingBerriesInChapter(int n, int chapter) {
            return Math.Min(1f, WingedBerryIDList.Count(id => id.Item1 == chapter && SaveData.Instance.Areas[id.Item1].Modes[0].Strawberries.Contains(new EntityID {Key = id.Item2})) / (float) n);
        }

        private static float HasNWingBerriesInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => mode == 0 ? HasNWingBerriesInChapter(n, ch) : 0f, chapters);
        }

        private static float HasSeekerStuns(int n) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.SeekerBonks.Sum() / (float) n);
        }

        private static float HasSeekerKillsInChapter(int n, int chapter, int mode) {
            var count = 0;
            foreach (var kill in BingoClient.Instance.ModSaveData.SeekerKills) {
                if (kill.StartsWith(chapter + "-" + mode)) {
                    count++;
                }
            }
            return Math.Min(1f, count / (float) n);
        }

        private static float HasSeekerKillsInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => HasSeekerKillsInChapter(n, ch, mode), chapters);
        }

        private static float HasSeekerStunsInChapter(int n, int chapter, int mode) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.SeekerBonks[chapter + mode*11] / (float) n);
        }

        private static float HasSeekerStunsInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => HasSeekerStunsInChapter(n, ch, mode), chapters);
        }

        private static float HasOshiroBonks(int n) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.OshiroBonks.Sum() / (float) n);
        }

        private static float HasOshiroBonksInChapter(int n, int chapter, int mode) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.OshiroBonks[chapter + mode*11] / (float) n);
        }

        private static float HasOshiroBonksInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => HasOshiroBonksInChapter(n, ch, mode), chapters);
        }

        private static float HasNSnowballs(int n) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.SnowballBonks.Sum() / (float) n);
        }

        private static float HasSnowballsInChapter(int n, int chapter, int mode) {
            return Math.Min(1f, BingoClient.Instance.ModSaveData.SnowballBonks[chapter + mode*11] / (float) n);
        }

        private static float HasSnowballsInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => HasSnowballsInChapter(n, ch, mode), chapters);
        }

        private static float HasNBinos(int n) {
            return Math.Min(1f, BinocularsList.Count / (float)n);
        }

        private static float HasNBinosInChapter(int n, int chapter, int mode) {
            return Math.Min(1f, BinocularsList.Count(bino => bino.areaID == chapter && bino.areaMode == mode) / (float) n);
        }

        private static float HasNBinosInASides(int n) {
            return Math.Min(1f, BinocularsList.Count(bino => bino.areaMode == 0 && bino.areaID != 10) / (float) n);
        }

        private static float HasNBinosInBSides(int n) {
            return Math.Min(1f, BinocularsList.Count(bino => bino.areaMode == 1) / (float) n);
        }

        private static float HasNBinosInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => Math.Min(1f, BinocularsList.Count(bino => bino.areaID == ch && bino.areaMode == mode) / (float) n), chapters);
        }

        private static float HasNCassettes(int n) {
            return Math.Min(1f, SaveData.Instance.Areas.Select(area => area.Cassette ? 1f : 0f).Sum() / n);
        }

        private static float HasChapterComplete(int chapter, int mode) {
            return SaveData.Instance.Areas[chapter].Modes[0].Completed ? 1f : 0f;
        }

        private static float HasChaptersComplete(int n) {
            return HasInNChapters(HasChapterComplete, n);
        }

        private static float HasChapterClear(int chapter) {
            return SaveData.Instance.Areas[chapter].Modes[0].FullClear ? 1f : 0f;
        }

        private static float HasChaptersClear(int n) {
            return HasInNChapters((ch, mode) => mode != 0 ? 0f : HasChapterClear(ch), n);
        }

        private static float HasNHearts(int n) {
            return Math.Min(1f, SaveData.Instance.Areas.Select(a => a.Modes.Count(m => m.HeartGem)).Sum() / (float) n);
        }

        private static float HasNASides(int n) {
            return Math.Min(1f, SaveData.Instance.Areas.Select(area => area.Modes[0].Completed && area.ID != 0 && area.ID != 8 && area.ID != 10 ? 1f : 0f).Sum() / n);
        }

        private static float HasNChapters(int n) {
            return HasInNChapters((ch, mode) => SaveData.Instance.Areas[ch].Modes[mode].Completed ? 1f : 0f, n);
        }

        private static float HasNSummitGems(int n) {
            if (SaveData.Instance.SummitGems == null) {
                return 0f;
            }
            return Math.Min(1f, SaveData.Instance.SummitGems.Select(x => x ? 1f : 0f).Sum() / n);
        }

        private static float HasSummitGems(params int[] indices) {
            if (SaveData.Instance.SummitGems == null) {
                return 0f;
            }
            return indices.Select(idx => SaveData.Instance.SummitGems[idx] ? 1f : 0f).Sum() / indices.Length;
        }

        private static float HasNBerries(int n) {
            return Math.Min(1f, SaveData.Instance.TotalStrawberries / (float)n);
        }

        private static float HasNHeartsColor(int n, int color) {
            return Math.Min(1f, SaveData.Instance.Areas.Select(area => area.Modes.Length > color && area.Modes[color] != null && area.Modes[color].HeartGem ? 1f : 0f).Sum() / n);
        }

        private static float HasNBerriesInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => mode == 0 ? HasNBerriesInChapter(n, ch) : 0f, chapters);
        }

        private static float HasNKeysInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => {
                if (ch == 3 && mode == 0) {
                    return HasNFlags(n, Keys3A);
                } else if (ch == 5 && mode == 0) {
                    return HasNFlags(n, Keys5A);
                } else if (ch == 5 && mode == 1) {
                    return HasNFlags(n, Keys5B);
                } else if (ch == 10 && mode == 0) {
                    return HasNFlags(n, KeysFW);
                } else {
                    return 0f;
                }
            }, chapters);
        }

        private static float HasInNChapters(Func<int, int, float> predicate, int chapters) {
            var map = new[] {1, 2, 3, 4, 5, 6, 7, 9, 10}.Select(ch => predicate(ch, 0)).ToList();
            map.AddRange(new[] {1, 2, 3, 4, 5, 6, 7, 9}.Select(ch => predicate(ch, 1)));
            map.Sort();
            map.Reverse();
            return map.Take(chapters).Sum() / chapters;
        }

        private static float HasCheckpoint(int chapter, int mode, int checkpoint) {
            return Math.Min(1f, SaveData.Instance.Areas[chapter].Modes[mode].Checkpoints.Count / (float)checkpoint);
        }

        private static float HasNBerriesInChapter(int n, int chapter, bool goldenCounts=true) {
            var berries = SaveData.Instance.Areas[chapter].Modes[0].TotalStrawberries;
            if (!goldenCounts) {
                berries -= SaveData.Instance.Areas[chapter].Modes[0].Strawberries.Count(
                    eid => AreaData.Areas[chapter].Mode[0].MapData.Get(eid.Level).Entities.First(
                        e => e.ID == eid.ID
                    ).Name != "strawberry"
                );
            }
            return Math.Min(1f, berries / (float)n);
        }

        private static float HasCassette(int chapter) {
            return SaveData.Instance.Areas[chapter].Cassette ? 1f : 0f;
        }

        private static float HasHeart(int chapter, int color) {
            return SaveData.Instance.Areas[chapter].Modes[color].HeartGem ? 1f : 0f;
        }

        private static float HasCheckpointBerries(int chapter, int checkpoint) {
            var dim = AreaData.Areas[chapter].Mode[0].StrawberriesByCheckpoint.GetLength(1);
            var possible = 0;
            var collected = 0;
            for (int i = 0; i < dim; i++) {
                var e = AreaData.Areas[chapter].Mode[0].StrawberriesByCheckpoint[checkpoint, i];
                if (e == null) {
                    continue;
                }

                possible++;
                if (SaveData.Instance.Areas[chapter].Modes[0].Strawberries.Contains(new EntityID {ID = e.ID, Level = e.Level.Name})) {
                    collected++;
                }
            }

            if (possible == 0) {
                // why are you here
                return 0f;
            }
            return (float)collected / possible;
        }

        public static float HasParticularStrawberries(int area, params string[] entities) {
            return entities.Count(e =>
                SaveData.Instance.Areas[area].Modes[0].Strawberries.Contains(new EntityID {Key = e})
            ) / (float) entities.Length;
        }
        #endregion

        #region variants
        public static IEnumerable<BingoVariant> EnabledVariants() {
            return typeof(BingoVariant).GetEnumValues().Cast<BingoVariant>().Where(IsVariantEnabled);
        }

        public static bool IsVariantEnabled(BingoVariant variant) {
            if (SaveData.Instance == null) {
                return false;
            }

            var extvar = ExtendedVariants.Module.ExtendedVariantsModule.Settings;
            switch (variant) {
                case BingoVariant.NoGrab:
                    return SaveData.Instance.VariantMode && SaveData.Instance.Assists.NoGrabbing;
                case BingoVariant.NoJump:
                    return extvar.MasterSwitch && ExtendedVariantInterop.JumpCount == 0 && ExtendedVariantInterop.DisableClimbJumping && ExtendedVariantInterop.DisableNeutralJumping && ExtendedVariantInterop.DisableWallJumping;
                case BingoVariant.NoDash:
                    return extvar.MasterSwitch && ExtendedVariantInterop.DashCount == 0;
                case BingoVariant.Invisible:
                    return SaveData.Instance.VariantMode && SaveData.Instance.Assists.InvisibleMotion;
                case BingoVariant.LowFriction:
                    return SaveData.Instance.VariantMode && SaveData.Instance.Assists.LowFriction;
                case BingoVariant.Speed70:
                    return SaveData.Instance.VariantMode && SaveData.Instance.Assists.GameSpeed == 7;
                case BingoVariant.Speed160:
                    return SaveData.Instance.VariantMode && SaveData.Instance.Assists.GameSpeed == 16;
                case BingoVariant.NoJumpNoDash:
                    return IsVariantEnabled(BingoVariant.NoDash) && IsVariantEnabled(BingoVariant.NoJump);
                case BingoVariant.Mirrored:
                    return SaveData.Instance.VariantMode && SaveData.Instance.Assists.MirrorMode;
                case BingoVariant.Hiccups:
                    return SaveData.Instance.VariantMode && SaveData.Instance.Assists.Hiccups;
            }

            return false;
        }

        public static void SetVariantEnabled(BingoVariant variant, bool enabled) {
            if (SaveData.Instance == null) {
                return;
            }

            var extvar = ExtendedVariants.Module.ExtendedVariantsModule.Settings;
            var extvarmod = ExtendedVariants.Module.ExtendedVariantsModule.Instance;
            switch (variant) {
                case BingoVariant.NoGrab:
                    if (enabled) SaveData.Instance.VariantMode = true;
                    SaveData.Instance.Assists.NoGrabbing = enabled;
                    break;
                case BingoVariant.NoJump:
                    if (enabled) {
                        extvar.MasterSwitch = true;
                        extvarmod.HookStuff();
                    }
                    ExtendedVariantInterop.JumpCount = enabled ? 0 : 1;
                    ExtendedVariantInterop.DisableClimbJumping = enabled;
                    ExtendedVariantInterop.DisableNeutralJumping = enabled;
                    ExtendedVariantInterop.DisableWallJumping = enabled;
                    break;
                case BingoVariant.NoDash:
                    if (enabled) {
                        extvar.MasterSwitch = true;
                        extvarmod.HookStuff();
                    }
                    ExtendedVariantInterop.DashCount = enabled ? 0 : -1;
                    if (enabled) {
                        var player = Engine.Scene.Tracker.GetEntity<Player>();
                        if (player != null) {
                            player.Dashes = 0;
                        }
                    }
                    break;
                case BingoVariant.Invisible:
                    if (enabled) SaveData.Instance.VariantMode = true;
                    SaveData.Instance.Assists.InvisibleMotion = enabled;
                    break;
                case BingoVariant.LowFriction:
                    if (enabled) SaveData.Instance.VariantMode = true;
                    SaveData.Instance.Assists.LowFriction = enabled;
                    break;
                case BingoVariant.Speed70:
                    if (enabled) SaveData.Instance.VariantMode = true;
                    SaveData.Instance.Assists.GameSpeed = enabled ? 7 : 10;
                    break;
                case BingoVariant.Speed160:
                    if (enabled) SaveData.Instance.VariantMode = true;
                    SaveData.Instance.Assists.GameSpeed = enabled ? 16 : 10;
                    break;
                case BingoVariant.NoJumpNoDash:
                    SetVariantEnabled(BingoVariant.NoJump, enabled);
                    SetVariantEnabled(BingoVariant.NoDash, enabled);
                    break;
                case BingoVariant.Mirrored:
                    if (enabled) SaveData.Instance.VariantMode = true;
                    SaveData.Instance.Assists.MirrorMode = enabled;
                    break;
                case BingoVariant.Hiccups:
                    if (enabled) SaveData.Instance.VariantMode = true;
                    SaveData.Instance.Assists.Hiccups = enabled;
                    break;
            }
        }

        public static int? AtCheckpoint() {
            if (SaveData.Instance?.CurrentSession == null) {
                return null;
            }

            var level = Engine.Scene as Level;
            var player = level?.Tracker.GetEntity<Player>();
            if (player == null) {
                return null;
            }

            bool first = ReferenceEquals(level.Session.LevelData, level.Session.MapData.StartLevel());

            var checkpoint = level.Entities.FindFirst<Checkpoint>();
            if (!first && checkpoint == null) {
                return null;
            }

            Vector2? refpoint1 = checkpoint?.Position;
            Vector2? refpoint2 = refpoint1 != null ? level.Session.GetSpawnPoint(refpoint1.Value + checkpoint.SpawnOffset) : level.Session.LevelData.Spawns[0];
            Vector2? refpoint3 = null;
            if (level.Session.Area == new AreaKey(6)) {
                refpoint3 = new Vector2(0, 300);
            }
            Vector2? refpoint4 = null;
            if (level.Session.Area == new AreaKey(6)) {
                refpoint4 = new Vector2(-1113, -225);
            }

            if ((refpoint1 == null || (refpoint1.Value - player.Position).LengthSquared() > 30 * 30) &&
                (refpoint2 == null || (refpoint2.Value - player.Position).LengthSquared() > 30 * 30) &&
                (refpoint3 == null || (refpoint3.Value - player.Position).LengthSquared() > 30 * 30) &&
                (refpoint4 == null || (refpoint4.Value - player.Position).LengthSquared() > 30 * 30)) {
                return null;
            }

            if (first) {
                return 0;
            }

            return IsCheckpointRoom(level.Session.Level);
        }

        public static int? IsCheckpointRoom(string room) {
            var level = Engine.Scene as Level;
            if (level == null) {
                return null;
            }

            var checkpointList = AreaData.Get(level.Session.Area)
                .Mode[(int) level.Session.Area.Mode]
                .Checkpoints;
            if (checkpointList == null) {
                return null;
            }
            var list = checkpointList
                .Where(ch => ch != null)
                .Select(ch => ch.Level)
                .ToList();
            if (!list.Contains(room)) {
                return null;
            }

            if (BingoWatches.IsSecretlyInSearch(level.Session)) {
                return null;
            }

            return list.IndexOf(room) + 1;
        }

        public static int CountCheckpoints(AreaKey area) {
            var areadata = AreaData.Get(area);
            var mode = (int) area.Mode;
            if (areadata.Mode.Length <= mode) {
                return 1;
            }
            return (areadata.Mode[mode].Checkpoints?.Length ?? 0) + 1;
        }

        #endregion
    }
}
