using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.BingoUI;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BingoClient {
    public partial class BingoClient {
        public static string[] TheoCutscenes = { "cutscene:1:6zb", "cutscene:2:end_2", "cutscene:3:09-d", "cutscene:search" };
        public static string[] SearchKeys = { "key:5:0:d-15:216", "key:5:0:d-04:39", "key:5:0:d-04:14" };
        public static string[] PowerSourceKeys = { "key:10:0:d-01:261", "key:10:0:d-02:70", "key:10:0:d-03:315", "key:10:0:d-04:444", "key:10:0:d-05:593" };
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
            { "Talk to Theo in Awake", () => HasFlag("cutscene:2:end_2") },
            { "All Berries in Awake (1)", () => HasCheckpointBerries(2, 2) },
            { "All Berries in Intervention (8)", () => HasCheckpointBerries(2, 1) },
            { "Complete Awake without dashing", null }, // generated
            { "All Berries in Start of 2A (9)", () => HasCheckpointBerries(2, 0) },
            { "Old Site Cassette", () => HasCassette(2) },
            { "Complete Chasm without dashing", null }, // generated
            { "5 Berries in 3 Chapters", () => HasNBerriesInChapters(5, 3) },
            { "Read the Poem in 2A", () => HasFlag("cutscene:2:end_s1") },
            { "Talk to Theo in Elevator Shaft", () => HasFlag("cutscene:3:09-d") },
            { "Complete Intervention without jumping", null }, // generated
            { "All Berries in Crossing (9)", () => HasCheckpointBerries(1, 1) },
            { "10 Berries in 2A", () => HasNBerriesInChapter(10, 2) },
            { "Find Letter and PICO-8 in Huge Mess", () => (HasFlag("cutscene:3:11-a") + HasFlag("foundpico")) / 2f },
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
            { "All Berries in Presidential Suite (3)", () => HasCheckpointBerries(3, 2) },
            { "Mirror Temple Cassette", () => HasCassette(5) },
            { "Huge Mess: Chest -> Books -> Towel", () => HasHugeMessOrder(2, 1, 0) },
            { "Huge Mess: Chest -> Towel -> Books", () => HasHugeMessOrder(2, 0, 1) },
            { "Huge Mess: Towel -> Books -> Chest", () => HasHugeMessOrder(1, 1, 2) },
            { "2 Seeded Berries", () => HasNSeedBerries(2) },
            { "2 optional Theo Cutscenes", () => HasNFlags(2, TheoCutscenes) },
            { "Get a 1-Up in 3 Chapters", () => HasN1upsInChapters(1, 3) },
            { "Read Diary in Elevator Shaft", () => HasFlag("cutscene:3:02-c") },
            { "Grabless Elevator Shaft", null }, // generated
            { "All Berries in Start of 4A (8)", () => HasCheckpointBerries(4, 0) },
            { "Golden Ridge Cassette", () => HasCassette(4) },
            { "Complete 3 A-Sides", () => HasNASides(3) },
            { "All Berries in Old Trail (7)", () => HasCheckpointBerries(4, 2) },
            { "3 Winged Berries", () => HasNWingBerries(3) },
            { "4 Winged Berries", () => HasNWingBerries(4) },
            { "Use 5 Binoculars in B-Sides", () => HasNBinosInBSides(5) },
            { "5 Berries in 5 Chapters", () => HasNBerriesInChapters(5, 5) },
            { "Grabless Huge Mess", null }, // generated
            { "Huge Mess: Books -> Towel -> Chest", () => HasHugeMessOrder(1, 0, 2) },
            { "Huge Mess: Books -> Chest -> Towel", () => HasHugeMessOrder(1, 2, 0) },
            { "Huge Mess: Towel -> Chest -> Books", () => HasHugeMessOrder(0, 2, 1) },
            { "Jump on 10 Snowballs", () => HasNSnowballs(10) },
            { "Grabless Presidential Suite", null }, // generated
            { "25 Berries", () => HasNBerries(25) },
            { "Golden Ridge Blue Heart", () => HasHeart(4, 0) },
            { "Grabless Cliff Face", null }, // generated
            { "4 Blue Hearts", () => HasNHeartsColor(4, 0) },
            { "Find Theo's Phone in 5A", () => HasFlag("cutscene:5:a-00c") },
            { "Celestial Resort Cassette", () => HasCassette(3) },
            { "Grabless Unraveling", null }, // generated
            { "Get 2 Keys in 5B", () => (HasFlag("key:5:1:b-02:221") + HasFlag("key:5:1:b-02:219")) / 2f },
            { "All Berries in Cliff Face (5)", () => HasCheckpointBerries(4, 3) },
            { "Grabless Search", null }, // generated
            { "5 Winged Berries", () => HasNWingBerries(5) },
            { "Talk to Theo in Search", () => HasFlag("cutscene:5:e-00") },
            { "4 Cassettes", () => HasNCassettes(4) },
            { "15 Berries in 4A", () => HasNBerriesInChapter(15, 4) },
            { "Use 6 Binoculars in B-Sides", () => HasNBinosInBSides(6) },
            { "Get 10 Berries in PICO-8", () => HasPicoBerries(10) },
            { "Stun Oshiro 10 times", () => HasOshiroBonks(10) },
            { "Golden Ridge B-Side", () => HasHeart(4, 1) },
            { "Winged Golden Berry", () => HasParticularStrawberries(1, "end:4") },
            { "Mirror Temple A-Side", () => HasASide(5) },
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
            { "Get 1 Key in Power Source", () => HasNFlags(1, PowerSourceKeys) },
            { "Reach Library (3B Checkpoint)", () => HasCheckpoint(3, 1, 2) },
            { "All Berries in Into the Core (1)", () => HasCheckpointBerries(9, 1) },
            { "Reflection Cutscene in Hollows", () => HasFlag("cutscene:6:04d") },
            { "Grabless Lake", null }, // generated
            { "35 Berries", () => HasNBerries(35) },
            { "40 Berries", () => HasNBerries(40) },
            { "Complete PICO-8", () => HasFlag("pico_complete") },
            { "3 optional Theo Cutscenes", () => HasNFlags(3, TheoCutscenes) },
            { "Kill a Seeker", () => HasSeekerKills(1) },
            { "Use 1 Binocular in 4 Chapters", () => HasNBinosInChapters(1, 4) },
            { "Only top route in Hollows", () => HasFlag("room:hollows:top") },
            { "Get 1 Key in Search", () => HasNFlags(1, SearchKeys) },
            { "Reflection Cassette", () => HasCassette(6) },
            { "Reflection Blue Heart", () => HasHeart(6, 0) },
            { "3 Seeded Berries", () => HasNSeedBerries(3) },
            { "Complete 2 A-Sides and 2 B-Sides", () => (HasNASides(2) + HasNHeartsColor(2, 1)) / 2f },
            { "5 Cassettes", () => HasNCassettes(5) },
            { "Stun Seekers 15 times", () => HasSeekerStuns(15) },
            { "Get 2 Keys in Power Source", () => HasNFlags(2, PowerSourceKeys) },
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
            { "Get 2 Keys in Search", () => HasNFlags(2, SearchKeys) },
            { "Get 3 Keys in Search", () => HasNFlags(3, SearchKeys) },
            { "6 Winged Berries", () => HasNWingBerries(6) },
            { "7 Winged Berries", () => HasNWingBerries(7) },
            { "Kill 2 different Seekers", () => HasSeekerKills(2) },
            { "Get 3 Keys in Power Source", () => HasNFlags(3, PowerSourceKeys) },
            { "Get 4 Keys in Power Source", () => HasNFlags(4, PowerSourceKeys) },
            { "Use 1 Binocular in 5 Chapters", () => HasNBinosInChapters(1, 5) },
            { "10 Berries in 3 Chapters", () => HasNBerriesInChapters(10, 3) },
            { "Switch to Ice on the right of Into the Core", () => HasFlag("first_ice") },
            { "5 Blue Hearts", () => HasNHeartsColor(5, 0) },
            { "Complete 3 B-Sides", () => HasNHeartsColor(3, 1) },
            { "3 Blue and 3 Red Hearts", () => (HasNHeartsColor(3, 0) + HasNHeartsColor(3, 1)) / 2f },
            { "Complete 2 Chapters Grabless", () => HasChaptersVariant(2, BingoVariant.NoGrab) },
            { "Stun Seekers 20 times", () => HasSeekerStuns(20) },
            { "Kill 3 different Seekers", () => HasSeekerKills(3) },
            { "Blue and Red Heart in Golden Ridge", () => (HasHeart(4, 0) + HasHeart(4, 1)) / 2f },
            { "Grabless Hollows", null }, // generated
            { "15 Berries in 2 Chapters", () => HasNBerriesInChapters(15, 2) },
            { "15 Berries in 3A", () => HasNBerriesInChapter(15, 3) },
            { "45 Berries", () => HasNBerries(45) },
            { "50 Berries", () => HasNBerries(50) },
            { "Reach Rock Bottom (6A/6B Checkpoint)", () => Math.Min(1f, HasCheckpoint(6, 0, 4) + HasCheckpoint(6, 1, 2)) },
            { "Use 2 Binoculars in 4 Chapters", () => HasNBinosInChapters(2, 4) },
            { "Blue and Red Heart in Mirror Temple", () => (HasHeart(5, 0) + HasHeart(5, 1)) / 2f },
            { "All Berries in Heart of the Mountain (1)", () => HasCheckpointBerries(9, 3) },
            { "Use all Binoculars in 500M (3)", () => HasParticularBinos(7, 0, "b-01", "b-02", "b-02b") },
            { "All Berries in 0M (4)", () => HasCheckpointBerries(7, 0) },
            { "Reflection A-Side", () => HasASide(6) },
            { "All Collectibles in 4A", () => (HasNBerriesInChapter(4, 29, false)*29f + HasCassette(4) + HasHeart(4, 0)) / 31f },
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
            { "Complete 4 B-Sides", () => HasNHeartsColor(4, 1) },
            { "Reflection B-Side", () => HasHeart(6, 1) },
            { "Reach the Intro Car in Remembered", () => HasFlag("remembered_intro_car") },
            { "5 Gems in the Summit", () => HasNSummitGems(5) },
            { "2000M and 2500M Gems", () => HasSummitGems(4, 5) },
            { "15 Berries in 3 Chapters", () => HasNBerriesInChapters(15, 3) },
            { "Grabless 6A", null }, // generated
            { "All Berries in 1500M (8)", () => HasCheckpointBerries(7, 3) },
            { "All Berries in 2000M (8)", () => HasCheckpointBerries(7, 4) },
            { "Get 5 Keys in Power Source", () => HasNFlags(5, PowerSourceKeys) },
            { "35 Berries in 7A", () => HasNBerriesInChapter(35, 7) },
            { "Blue and Red Heart in Reflection", () => (HasHeart(6, 0) + HasHeart(6, 1)) / 2f },
            { "All Flags in 3000M", () => HasFlag("all_summit_flags") },
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
            { "All Collectibles in 8A", () => (HasNBerriesInChapter(5, 9, false)*5f + HasCassette(9) + HasHeart(9, 0)) / 7f },
            { "The Summit Blue Heart", () => HasHeart(7, 0) },

            { "2 Winged Berries", () => HasNWingBerries(2) },
            { "Take hidden path before Cliff Face", () => HasFlag("room:oldtrailsecret") },
            { "Complete Awake without jumping", null }, // generated
            { "2 Hearts", () => HasNHearts(2) },
            { "Complete 1 B-Side", () => HasNHeartsColor(1, 1) },
            { "Use 3 Binoculars in B-Sides", () => HasNBinosInBSides(3) },
            { "2 Hearts and 2 Cassettes", () => (HasNHearts(2) + HasNCassettes(2)) / 2 },
            { "1 Blue and 1 Red Heart", () => (HasNHeartsColor(1, 0) + HasNHeartsColor(1, 1)) / 2 },
            { "Use all Binoculars in 4A (3)", () => HasNBinosInChapter(3, 4, 0) },
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
            { "Use 1 Binocular in 3 Chapters", () => HasNBinosInChapters(1, 3) },
            { "Grabless Rescue", null }, // generated
            { "3 Hearts and 3 Cassettes", () => (HasNHearts(3) + HasNCassettes(3)) / 2 },
            { "4 Hearts and 4 Cassettes", () => (HasNHearts(4) + HasNCassettes(4)) / 2 },
            { "Stun Seekers 10 times", () => HasSeekerStuns(10) },
            { "15 Berries in 5A", () => HasNBerriesInChapter(15, 5) },
            { "Don't skip final 4A Cutscene", () => HasFlag("cutscene:4:d-10") },
            { "Use 3 Binoculars in 2 Chapters", () => HasNBinosInChapters(3, 2) },
            { "Reach Rock Bottom (6B Checkpoint)", () => HasCheckpoint(6, 1, 2) },
            { "20 Berries in 4A", () => HasNBerriesInChapter(20, 4) },
            { "Complete Resolution without jumping", null }, // generated
            { "Empty Space", () => HasFlag("empty_space") },
            { "Clear Core", () => HasASide(9) },

            { "Triple 1-Up", null },
            { "Crossing Dashless", null },
            { "Jumpless Dashless Awake", null },
            { "Complete Forsaken City with Low Friction", null },
            { "2 Chapters Grabless", null },
            { "Two Blue and Two Red Hearts", null },
            { "3 B-Sides", () => HasNHeartsColor(3, 1) },
            { "Complete Forsaken City with 70% Speed", null },
            { "Complete Forsaken City with 160% Speed", null },
            { "Complete Old Site with Low Friction", null },
            { "Full Clear 1A", null },
            { "Quintuple 1-Up", null },
            { "Winged Golden", null },
            { "Complete 2 Chapters Mirrored", null },
            { "Complete Pico-8", null },
            { "Get a 1-Up in 4 Chapters", () => HasN1upsInChapters(1, 4) },
            { "Complete Intervention without Jumping", null }, // generated
            { "Complete Old Trail with Low Friction", null },
            { "100% Pico-8", null },
            { "Complete Presidential Suite with Low Friction", null },
            { "Bop Oshiro 10 Times in Two Chapters", null },
            { "All Berries in Start and Depths of 5A (23)", null },
            { "Invisible Huge Mess", null },
            { "Complete Elevator Shaft with Low Friction", null },
            { "Complete 4 Chapters Mirrored", null },
            { "Get 4 Hearts and 4 Cassettes", null },
            { "Invisible Forsaken City", null },
            { "4 Chapters Hiccups", null },
            { "Get a 1-Up in 3A", () => HasN1upsInChapter(1, 3) },
            { "Bop Seekers 10 Times in 2 Chapters", null },
            { "Complete 3A Start with Low Friction", null },
            { "Invisible Unravelled", null },
            { "Invisible Cliff Face", null },
            { "Complete Start of Mirror Temple with Low Friction", null },
            { "Invisible Start of Celestial Resort", null },
            { "Invisible Old Site", null },
            { "Grabless Huge Mess with the Heart", null },
            { "Complete 3 Chapters with Low Friction", null },
            { "Full Clear 8A", null },
            { "Grabless Celestial Resort", null },
            { "Kill 5 Different Seekers", () => HasSeekerKills(5) },
            { "Complete Mirror Temple with Low Friction", null },
            { "Kill 3 Different Seekers in Two Chapters", null },
            { "Get 15 Berries in 4 Chapters", () => HasNBerriesInChapters(15, 4) },
            { "Intro Car in Remembered", null },
            { "1-Up in 7A", () => HasN1upsInChapter(1, 7) },
            { "Jumpless Reflection (Checkpoint)", null },
            { "Jumpless Resolution", null },
            { "Grabless Mirror Temple", null },
            { "Invisible Mirror Temple", null },
            { "Grabless Repreive", null },
            { "5 Berries in 7 Chapters", () => HasN1upsInChapters(5, 7) },
            { "Invisible Presidential Suite", null },
            { "Bounce on 10 Snowballs in 2 Chapters", null },
            { "3000M Grabless", null },
            { "Complete 2 Chapters Invisible", null },
            { "Invisible 1500M", null },
            { "Invisible 2500M", null },
            { "Every Different Seeker Kill (12)", null }, // do the two seekers in the same room count?
            { "Complete 5 Chapters Grabless", null },
            { "Use 4 Binoculars in 8 Chapters", () => HasNBinosInChapters(4, 8) },
            { "Full Clear Three Chapters", null },
            { "Invisible 3000M", null },
            { "69 Berries", () => HasNBerries(69) },
            { "Blue Heart in The Summit", () => HasHeart(7, 0) },
            { "Find the Bird's Nest in Epilogue", () => HasFlag("room:birdnest") },
            { "20 Berries in 3 Chapters", () => HasNBerriesInChapters(20, 3) },
            { "Invisible Power Source", null },
            { "Complete 5 Chapters with Low Friction", null },
            { "Complete 3000M with no Badeline Orbs", null },
            { "20 Berries in 4 Chapters", () => HasNBerriesInChapters(20, 4) },
            { "Use 2 Binoculars in 8 Chapters", () => HasNBinosInChapters(2, 8) },
            { "All Winged Berries (11)", () => HasNWingBerries(11) },
            { "Complete 2B Grabless", null },
            { "Complete Rooftop Jumpless", null },
            { "Complete 6 Chapters Grabless", null },
            { "Reach Event Horizon", () => HasCheckpoint(10, 0, 4) },
            { "Use All Binoculars in A-Sides (14)", () => HasNBinosInASides(14) },
            { "Complete 6 Chapters Invisible", null },
            { "Use All Binoculars in B-Sides (13)", () => HasNBinosInBSides(13) },
            { "Use 20 Binoculars in Farewell", () => HasNBinosInChapter(20, 10, 0) },
            { "10 Hearts", () => HasNHearts(10) },
            { "6 Hearts and 6 Cassettes", () => (HasNHearts(6) + HasNCassettes(6)) / 2f },
            { "Full Clear 7A", null },
            { "8 Cassettes", () => HasNCassettes(8) },
            { "Blue and Red Heart in The Summit", () => (HasHeart(7, 0) + HasHeart(7, 1)) / 2f },
            { "Complete Remembered with Low Friction", null },
            { "25 Berries in 4 Chapters", () => HasNBerriesInChapters(25, 4) },
            { "Determination Demo Room", null },
            { "20 Berries in 5 Chapters", () => HasNBerriesInChapters(20, 5) },
            { "Complete 10 Chapters", null },
            { "4 A-Sides and 4 B-Sides", null },
            { "12 Hearts", () => HasNHearts(12) },
            { "Unlock Reconciliation", () => HasCheckpoint(10, 0, 7) },
            { "Five Full Clears", null },
            { "Complete 6 B-Sides", () => HasNHeartsColor(6, 1) },
            { "Grabless 6B", null },
            { "Moon Berry", null },
            { "125 Berries", () => HasNBerries(125) },
            { "All Blue Hearts", () => HasNHeartsColor(8, 0) },
            { "7 Red Hearts", () => HasNHeartsColor(7, 1) },
            { "Farewell", () => HasASide(10) },

        };

        private static float HasParticularBinos(int chapter, int mode, params string[] levels) {
            return levels.Count(lvl => BinocularsList.Any(bino => {
                var point = new Point((int) bino.pos.X, (int) bino.pos.Y);
                return bino.areaID == chapter && bino.areaMode == mode && AreaData.Areas[chapter].Mode[mode].MapData.Get(lvl).Bounds.Contains(point);
            })) / (float) levels.Length;
        }

        private static float HasNFlags(int n, params string[] flags) {
            return Math.Min(1f, flags.Count(flag => Instance.ModSaveData.FileFlags.Contains(flag)) / (float) n);
        }

        public static Dictionary<string, List<Tuple<int, int, int, BingoVariant>>> ObjectiveVariants = new Dictionary<string, List<Tuple<int, int, int, BingoVariant>>> {
            { "Complete 1A Start without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 0, BingoVariant.NoDash)} },
            { "Complete Crossing without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 1, BingoVariant.NoDash)} },
            { "Complete Chasm without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 2, BingoVariant.NoDash)} },
            { "Complete Awake without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(2, 0, 2, BingoVariant.NoDash)} },
            { "Complete Shrine without dashing", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(4, 0, 1, BingoVariant.NoDash)} },
            { "Complete 1A Start without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(1, 0, 0, BingoVariant.NoJump)} },
            { "Complete Intervention without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(2, 0, 1, BingoVariant.NoJump)} },
            { "Complete Intervention without Jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(2, 0, 1, BingoVariant.NoJump)} },
            { "Complete Awake without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(2, 0, 2, BingoVariant.NoJump)} },
            { "Complete Start of 5A without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 0, BingoVariant.NoJump)} },
            { "Complete Resolution without jumping", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 5, BingoVariant.NoJump)} },
            { "Grabless Start of 3A", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 0, BingoVariant.NoGrab)} },
            { "Grabless Huge Mess", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 1, BingoVariant.NoGrab)} },
            { "Grabless Elevator Shaft", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 2, BingoVariant.NoGrab)} },
            { "Grabless Presidential Suite", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(3, 0, 3, BingoVariant.NoGrab)} },
            { "Grabless Start of 4A", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(4, 0, 0, BingoVariant.NoGrab)} },
            { "Grabless Cliff Face", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(4, 0, 3, BingoVariant.NoGrab)} },
            { "Grabless Depths", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 1, BingoVariant.NoGrab)} },
            { "Grabless Unraveling", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 2, BingoVariant.NoGrab)} },
            { "Grabless Search", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 3, BingoVariant.NoGrab)} },
            { "Grabless Rescue", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(5, 0, 4, BingoVariant.NoGrab)} },
            { "Grabless Lake", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 1, BingoVariant.NoGrab)} },
            { "Grabless Hollows", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 2, BingoVariant.NoGrab)} },
            { "Grabless Rock Bottom", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(6, 0, 4, BingoVariant.NoGrab)} },
            { "Grabless 1A", new List<Tuple<int, int, int, BingoVariant>> {
                Tuple.Create(1, 0, 0, BingoVariant.NoGrab),
                Tuple.Create(1, 0, 1, BingoVariant.NoGrab),
                Tuple.Create(1, 0, 2, BingoVariant.NoGrab),
            } },
            { "Grabless 2A", new List<Tuple<int, int, int, BingoVariant>> {
                Tuple.Create(2, 0, 0, BingoVariant.NoGrab),
                Tuple.Create(2, 0, 1, BingoVariant.NoGrab),
                Tuple.Create(2, 0, 2, BingoVariant.NoGrab),
            } },
            { "Grabless 3A", new List<Tuple<int, int, int, BingoVariant>> {
                Tuple.Create(3, 0, 0, BingoVariant.NoGrab),
                Tuple.Create(3, 0, 1, BingoVariant.NoGrab),
                Tuple.Create(3, 0, 2, BingoVariant.NoGrab),
                Tuple.Create(3, 0, 3, BingoVariant.NoGrab),
            } },
            { "Grabless 5A", new List<Tuple<int, int, int, BingoVariant>> {
                Tuple.Create(5, 0, 0, BingoVariant.NoGrab),
                Tuple.Create(5, 0, 1, BingoVariant.NoGrab),
                Tuple.Create(5, 0, 2, BingoVariant.NoGrab),
                Tuple.Create(5, 0, 3, BingoVariant.NoGrab),
                Tuple.Create(5, 0, 4, BingoVariant.NoGrab),
            } },
            { "Grabless 6A", new List<Tuple<int, int, int, BingoVariant>> {
                Tuple.Create(6, 0, 0, BingoVariant.NoGrab),
                Tuple.Create(6, 0, 1, BingoVariant.NoGrab),
                Tuple.Create(6, 0, 2, BingoVariant.NoGrab),
                Tuple.Create(6, 0, 3, BingoVariant.NoGrab),
                Tuple.Create(6, 0, 4, BingoVariant.NoGrab),
                Tuple.Create(6, 0, 5, BingoVariant.NoGrab),
            } },
            { "Complete 2 Chapters Grabless", new List<Tuple<int, int, int, BingoVariant>> {Tuple.Create(-1, -1, -1, BingoVariant.NoGrab)} },
        };

        private static void InitObjectives() {
            foreach (var kv in ObjectiveVariants) {
                if (Objectives.TryGetValue(kv.Key, out var maybenull) && maybenull != null) {
                    continue;
                }
                
                Func<float> close(List<Tuple<int, int, int, BingoVariant>> reqs) {
                    return () => {
                        if (SaveData.Instance == null) {
                            return 0f;
                        }

                        return reqs.Count(req => Instance.ModSaveData.HasCheckpointVariant(req.Item1, req.Item2, req.Item3, req.Item4)) / (float)reqs.Count;
                    };
                }
                Objectives[kv.Key] = close(kv.Value);
            }
        }

        #region checkers

        private static float HasChapterVariant(int chapter, int mode, BingoVariant variant) {
            var missed = false;
            var total = CountCheckpoints(new AreaKey(chapter, (AreaMode) mode));
            for (var ch = 0; ch < total; ch++) {
                if (!Instance.ModSaveData.HasCheckpointVariant(chapter, mode, ch, variant)) {
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
            return Instance.ModSaveData.FileFlags.Contains(flag) ? 1f : 0f;
        }
        
        private static float HasPicoBerries(int n) {
            return Math.Min(1f, Instance.ModSaveData.PicoBerries / (float) n);
        }

        private static float HasHugeMessOrder(int p0, int p1, int p2) {
            return Instance.ModSaveData.HasHugeMessOrder(p0, p1, p2) ? 1f : 0f;
        }

        private static float HasSeekerKills(int n) {
            return Math.Min(1f, Instance.ModSaveData.SeekerKills.Count / (float) n);
        }

        private static float HasN1upsInChapters(int n, int chapters) {
            return HasInNChapters((ch, mode) => mode == 0 ? HasN1upsInChapter(n, ch) : 0f, chapters);
        }

        private static float HasN1upsInChapter(int n, int chapter) {
            return Math.Min(1f, Instance.ModSaveData.OneUps[chapter] / (float) n);
        }

        private static float HasN1ups(int n) {
            return Math.Min(1f, Instance.ModSaveData.OneUps.Sum() / (float) n);
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
    
        private static FieldInfo BinosField = typeof(BingoModule).GetField("BinocularsList", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo SeekersField = typeof(BingoModule).GetField("SeekersHit", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo SnowballsField = typeof(BingoModule).GetField("SnowballHits", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo OshiroField = typeof(BingoModule).GetField("OshiroHits", BindingFlags.NonPublic | BindingFlags.Instance);
        public static List<Binoculars> BinocularsList => (List<Binoculars>)BinosField.GetValue(BingoModule.Instance);
        public static int SeekersHit => (int)SeekersField.GetValue(BingoModule.Instance);
        public static int SnowballHits => (int)SnowballsField.GetValue(BingoModule.Instance);
        public static int OshiroHits => (int)OshiroField.GetValue(BingoModule.Instance);
        
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

        private static float HasNBinos(int n) {
            return Math.Min(1f, BinocularsList.Count / (float)n);
        }
        
        private static float HasSeekerStuns(int n) {
            return Math.Min(1f, SeekersHit / (float) n);
        }
        
        private static float HasOshiroBonks(int n) {
            return Math.Min(1f, OshiroHits / (float) n);
        }
        
        private static float HasNSnowballs(int n) {
            return Math.Min(1f, SnowballHits / (float) n);
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

        private static float HasASide(int chapter) {
            return SaveData.Instance.Areas[chapter].Modes[0].Completed ? 1f : 0f;
        }

        private static float HasNHearts(int n) {
            return Math.Min(1f, SaveData.Instance.Areas.Select(a => a.Modes.Count(m => m.HeartGem)).Sum() / (float) n);
        }

        private static float HasNASides(int n) {
            return Math.Min(1f, SaveData.Instance.Areas.Select(area => area.Modes[0].Completed ? 1f : 0f).Sum() / n);
        }

        private static float HasNSummitGems(int n) {
            return Math.Min(1f, SaveData.Instance.SummitGems.Select(x => x ? 1f : 0f).Sum() / n);
        }

        private static float HasSummitGems(params int[] indices) {
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

        private static float HasInNChapters(Func<int, int, float> predicate, int chapters) {
            var map = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.Select(ch => predicate(ch, 0)).ToList();
            map.AddRange(new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.Select(ch => predicate(ch, 1)));
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

        public List<bool> ObjectivesCompleted;

        public void StartObjectives() {
            this.ObjectivesCompleted = new List<bool>();
            for (int i = 0; i < 25; i++) {
                this.ObjectivesCompleted.Add(false);
            }
        }

        public void UpdateObjectives() {
            if (this.ObjectivesCompleted == null) {
                return;
            }
            
            for (var i = 0; i < 25; i++) {
                if (this.GetObjectiveStatus(i) != ObjectiveStatus.Completed) {
                    continue;
                }

                if (!this.ObjectivesCompleted[i]) {
                    this.ObjectivesCompleted[i] = true;
                    Chat(string.Format(Dialog.Get("bingoclient_objective_claimable"), this.Board[i].Text));
                }
                
                if (this.ModSettings.QuickClaim.Check) {
                    this.SendClaim(i);
                }
            }
        }

        public ObjectiveStatus GetObjectiveStatus(int i) {
            if (this.Board[i].Color != Color.Black) {
                return ObjectiveStatus.Claimed;
            }

            if (this.ObjectivesCompleted[i]) {
                return ObjectiveStatus.Completed;
            }

            if (SaveData.Instance == null) {
                return ObjectiveStatus.Nothing;
            }
            
            if (!Objectives.TryGetValue(this.Board[i].Text, out var checker) || checker == null) {
                return ObjectiveStatus.Unknown;
            }
            
            var progress = checker();
            if (progress < 0.001f) {
                return ObjectiveStatus.Nothing;
            }

            if (progress > 0.999f) {
                return ObjectiveStatus.Completed;
            }

            return ObjectiveStatus.Progress;
        }

        public bool IsObjectiveClaimable(int i) {
            return this.Board[i].Color == Color.Black && this.ObjectivesCompleted[i];
        }

        public enum ObjectiveStatus {
            Nothing, Unknown, Progress, Completed, Claimed
        }
    }
}
