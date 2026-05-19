**WESTMINSTER**

AI Implementation PRD

*A complete specification for a UK-set grand strategy game,*

*structured for ingestion by AI coding assistants.*

Version 1.0 · May 2026

Personal hobby project · Single-player · UK start

Target engine: Godot 4.3 + C#

Table of Contents

  -------------------------------------------------------------------------
  **§**   **Section**                                            **Approx
                                                                 page**
  ------- ------------------------------------------------------ ----------
  0       Document Conventions & How AI Should Read This         p.3

  1       Executive Context                                      p.5

  2       Glossary of Terms                                      p.7

  3       System Architecture                                    p.9

  4       Data Models (JSON Schemas)                             p.13

  5       Game Systems --- Core Loop                             p.22

  6       Game Systems --- Character & Rise to Power             p.26

  7       Game Systems --- UK Governance Layer                   p.32

  8       Game Systems --- Policy Engine                         p.38

  9       Game Systems --- Ideology & Government Type            p.46

  10      Game Systems --- Pops, Factions & Elections            p.50

  11      Game Systems --- Schemes, Cabinet & Narrative          p.56

  12      Game Systems --- Media & Narrative                     p.62

  13      Game Systems --- Situations & Crises                   p.66

  14      Game Systems --- World & Foreign Policy                p.70

  15      Game Systems --- Multi-Generational Continuity         p.74

  16      UI Specifications                                      p.76

  17      Technical Architecture Details                         p.84

  18      Real-World Data Appendices                             p.88

  19      Development Roadmap (Phase Definitions)                p.96

  20      Test Cases & Acceptance Criteria                       p.100

  21      Risks & Mitigations                                    p.103

  22      Open Design Questions                                  p.105

  23      Source Citations                                       p.106
  -------------------------------------------------------------------------

0\. Document Conventions & How AI Should Read This

This PRD is engineered for ingestion by AI coding assistants. Sections
are self-contained where possible and use a consistent block structure.

0.1 Section Block Structure

Every game system in §§5--15 follows this exact block format:

+-----------------------------------------------------------------------+
| SPEC: \<system-id\> e.g. SPEC: character.attributes                   |
|                                                                       |
| Purpose: One-sentence statement of what this system does              |
|                                                                       |
| Inputs: Data this system consumes                                     |
|                                                                       |
| Outputs: Data this system produces                                    |
|                                                                       |
| Dependencies: Other systems this system requires                      |
|                                                                       |
| Out of scope: What this system does NOT do                            |
|                                                                       |
| Data model: JSON schema (see §4 for global schemas)                   |
|                                                                       |
| Behaviour: Pseudocode for the system\'s core algorithm                |
|                                                                       |
| UI contract: What UI components this system requires                  |
|                                                                       |
| Acceptance criteria: Bullet list of testable pass/fail conditions     |
|                                                                       |
| Test cases: At least 3 named scenarios with expected outcomes         |
+-----------------------------------------------------------------------+

0.2 Identifier Conventions

  -----------------------------------------------------------------------------------------------
  **Prefix**       **Meaning**                              **Example**
  ---------------- ---------------------------------------- -------------------------------------
  char\_           Named character (politician, journalist, char_starmer_keir
                   etc.)                                    

  constituency\_   Westminster constituency                 constituency_holborn_and_st_pancras

  lad\_            Local Authority District                 lad_camden

  country\_        Sovereign country (ISO 3166-1 alpha-2    country_fr
                   lowercase suffix preferred)              

  party\_          Political party                          party_labour

  faction\_        Interest group / faction                 faction_unions

  policy\_         Policy lever                             policy_income_tax_higher_rate

  ideology\_       Named ideology                           ideology_social_democracy

  scheme\_         Intrigue scheme template                 scheme_manufacture_scandal

  situation\_      Slow-burn situation                      situation_climate_change

  event\_          Atomic event                             event_runcorn_byelection_2025

  initiative\_     Strategic Initiative (focus-tree node)   initiative_uk_eu_rejoin

  chapter\_        Suzerain-style narrative chapter         chapter_first_leadership_bid
  -----------------------------------------------------------------------------------------------

0.3 Cross-Reference Convention

Cross-references use §N.M.P notation, e.g. \'§4.2.1 Character schema\'.
AI assistants should treat these as authoritative pointers --- if §4.2.1
says a field is required, every consuming system MUST treat it as
required.

0.4 Priority Tags

  ------------------------------------------------------------------------
  **Tag**   **Meaning**
  --------- --------------------------------------------------------------
  MVP       Required in Phase 1 (first 12 weeks). Cannot be skipped.

  P2        Required in Phase 2 (months 4--7).

  P3        Phase 3 (months 8--11).

  P4        Phase 4 (months 12--16).

  P5        Phase 5 (months 17+).

  NICE      Optional. May be skipped without affecting playability.

  WONT      Explicitly out of scope. Do not implement.
  ------------------------------------------------------------------------

0.5 Build Order for AI Assistants

If an AI assistant is asked to implement Westminster, the build order is
fixed:

1.  Set up the project skeleton (Godot 4.3 + C# project, folder
    structure per §17.1).

2.  Implement all data schemas from §4 as C# records with JSON
    serialisation.

3.  Implement the core simulation tick loop from §5 (no UI yet --- log
    to console).

4.  Build the character creation flow per §6.1, save/load to SQLite per
    §17.4.

5.  Implement the policy engine per §8 with the 50 MVP policies from
    §8.7.

6.  Implement the pop model per §10.1 (1,000 pops, 12 regions).

7.  Implement the election system per §10.5 with the FPTP simulator.

8.  Build the UK map per §16.2 using ONS BGC topology from §18.5.

9.  Implement the cabinet system per §11.4 with real May 2026 seed data
    from §18.1.

10. Implement the 5 MVP schemes per §11.1.

11. Polish, playtest, ship MVP.

+-----------------------------------------------------------------------+
| **Implementation rule for AI assistants**                             |
|                                                                       |
| Do not skip sections. Do not invent fields not in §4. Do not          |
| implement P2+ features before MVP ships. If a spec is ambiguous,      |
| prefer the simpler interpretation and add a comment noting the        |
| choice.                                                               |
+-----------------------------------------------------------------------+

0.6 What This Document Is Not

-   Not a game design document --- that\'s the Personal Build Guide.

-   Not a marketing pitch --- this is for a private hobby project, not
    commercialisation.

-   Not a finished UI/UX spec --- visual design is owner\'s choice
    during implementation.

-   Not a balanced economy spec --- numerical balancing happens in
    playtesting, not upfront.

1\. Executive Context

1.1 One-Sentence Specification

A single-player real-time-with-pause grand strategy game in which the
player creates one human character starting anywhere on the UK political
ladder, rises to power by any route, and transforms the United Kingdom
into any constitutional form using approximately 525 distinct policy
levers, against a fully simulated world of 195 named real-world heads of
state.

1.2 Inspirational Sources & What\'s Borrowed

  ------------------------------------------------------------------------
  **Source**      **Core mechanic borrowed**             **Used in §**
  --------------- -------------------------------------- -----------------
  Crusader Kings  Schemes, hooks, secrets, lifestyle     §6, §11, §15
  3               perks, dynasty legacies                

  Football        1--20 attribute system, hidden         §6.1, §6.3, §15.3
  Manager         attributes, scouting, regen            

  Democracy 4     Policy web visualisation, slider       §8, §16.7
                  intensity, voter group simulation      

  Suzerain        Stat-checked dialogue trees, cabinet   §11.5, §11.6
                  meetings, constitutional reform        

  Victoria 3      Pops with strata/profession/ideology,  §10.1, §10.2, §13
                  interest groups, journal entries       

  Stellaris       Situations system, ethics×factions,    §13
                  end-game crises                        

  Tropico 6       Faction approval, election speeches,   §10.4, §10.7
                  edicts, cancel-elections               

  Hearts of Iron  National focus trees rebadged as       §7.8
  4               Strategic Initiatives                  

  Shadow Empire   Council ministers with stats +         §11.4, §14.5
                  loyalty, stratagems, OHQ war           

  Civilization VI Government types, swappable policy     §9
                  cards                                  

  Old World       Shared Orders pool as universal        §5.4
                  political-capital currency             

  Headliner:      Editorial approve/reject media loop    §12
  NoviNews                                               

  Terra Invicta   Named agents with d100 missions on a   §14.6
                  world map                              
  ------------------------------------------------------------------------

1.3 Target Player Experience

A 60-hour campaign that begins with the player door-knocking in a
specific real constituency and ends, four in-game decades later, with
the player having either died peacefully as a respected statesman, been
deposed in a coup of their own making, or having abolished Parliament
and crowned themselves king.

1.4 Design Pillars (Inviolable)

-   P1. The simulation never lies. If a policy is illogical, it should
    fail visibly in the simulation rather than being blocked by a
    developer\'s value judgement.

-   P2. Every decision is a human decision. Abstractions resolve to
    named, faced characters who can be befriended, betrayed,
    blackmailed, or killed.

-   P3. Depth where it matters, breadth where it doesn\'t. UK layer is
    deep; world layer is broad-shallow.

-   P4. The real world is the canvas. Real names, real boundaries, real
    polling, real demographics --- accuracy is a feature.

-   P5. Player intent is sacred. If you can describe a policy in three
    words, the policy system should have a lever for it.

-   P6. History rhymes, it doesn\'t repeat. Procedural events draw from
    a deep deck rather than scripted sequences.

-   P7. The exit door is always open. You can always quit politics,
    become a businessman, found a media empire, retire and run your
    dynasty from the shadows.

1.5 Scope Boundaries

  -----------------------------------------------------------------------
  **In scope**                        **Out of scope (forever or for
                                      v1)**
  ----------------------------------- -----------------------------------
  Real UK political simulation, 2026  Multiplayer (out of scope v1)
  start                               

  Real world map, 195 countries       VR / mobile (out of scope forever)

  Real politicians named at start     Real-time 3D combat (out of scope
                                      forever)

  Policy levers across all domains    Sports / non-political simulation
                                      (out of scope)

  Multi-decade dynasty/party/network  Pre-1979 history (start dates are
  continuity                          2026 / January 2026 only)

  JSON modding API                    Day-one Steam Workshop (post-v1)
  -----------------------------------------------------------------------

2\. Glossary of Terms

Terms used throughout this document. AI assistants should treat these as
authoritative definitions.

  -----------------------------------------------------------------------
  **Term**        **Definition**
  --------------- -------------------------------------------------------
  Attribute       A 1--20 numeric stat on a character (Football Manager
                  style). Can be visible or hidden.

  Trait           A named modifier on a character (e.g., \'Oxford PPE\',
                  \'Drinker\'). Discrete, not numeric.

  Pop             A simulated population unit. \~1,000 in MVP, scaling to
                  \~6,000 by P5. Has region, stratum, ideology, etc.

  Stratum         Economic class of a pop (poor, middle, wealthy).

  Faction         An interest group representing pop interests (e.g.,
                  Unions, Big Business).

  Scheme          A multi-tick covert action with a target, progress
                  meter, and success/failure roll. CK3-derived.

  Hook            A piece of leverage held over a character. Weak (single
                  use) or Strong (until exposed).

  Secret          A hidden fact about a character. Can be discovered by
                  spies and converted to a hook.

  Situation       A slow-burn 0--100 tracked variable (e.g., climate, NHS
                  backlog) with stage milestones.

  Event           An atomic news/decision moment fired by the simulation.

  Initiative      A multi-year Strategic Initiative (HoI4-style focus).
                  Costs Orders per year.

  Order           The shared executive-bandwidth currency.
                  Old-World-derived. Caps actions per month.

  Chapter         A Suzerain-style hand-authored branching dialogue
                  scene, fired at rank promotions.

  IG              Interest Group. Vic3-derived. A political organisation
                  of pops.

  Career rank     0--12 ladder: Citizen → Activist → Councillor → ... →
                  PM → Statesman → Founder.

  BGC             ONS Boundaries: \'Generalised, Clipped\'. Recommended
                  simplification for UK map.

  FPTP            First-past-the-post. Default UK electoral system.

  Tick            One simulation step. Default = 1 in-game day. Pop sim
                  runs monthly.

  Newgen          Procedurally generated successor character. FM-derived.
                  Used for dynasty/party/network heirs.
  -----------------------------------------------------------------------

3\. System Architecture

3.1 Module Map

The codebase is organised into 11 modules. Dependencies flow downward
only.

+-----------------------------------------------------------------------+
| /src                                                                  |
|                                                                       |
| ├── /Core \# Module 0: data types, math utilities, RNG                |
|                                                                       |
| ├── /Simulation \# Module 1: tick loop, time, calendar                |
|                                                                       |
| ├── /World \# Module 2: countries, IOs, world events                  |
|                                                                       |
| ├── /UK \# Module 3: constituencies, LADs, devolved govs              |
|                                                                       |
| ├── /Pops \# Module 4: pop simulation, migration, ideology drift      |
|                                                                       |
| ├── /Policy \# Module 5: policy levers, effects, enactment            |
|                                                                       |
| ├── /Character \# Module 6: attributes, traits, lifecycle, schemes    |
|                                                                       |
| ├── /Faction \# Module 7: interest groups, endorsements               |
|                                                                       |
| ├── /Election \# Module 8: FPTP + alt systems, leadership contests    |
|                                                                       |
| ├── /Narrative \# Module 9: chapters, events, journal entries         |
|                                                                       |
| ├── /UI \# Module 10: all screens (Godot scenes)                      |
|                                                                       |
| └── /Persistence \# Module 11: save/load, SQLite, JSON loaders        |
+-----------------------------------------------------------------------+

3.2 Module Dependency Graph

+-----------------------------------------------------------------------+
| UI ──────────────────────────────────────────────────────┐            |
|                                                                       |
| │                                                                     |
|                                                                       |
| Narrative ─────────────┐ │                                            |
|                                                                       |
| ▼ │                                                                   |
|                                                                       |
| Election ──► Faction ──► Character │                                  |
|                                                                       |
| │ │ │ │                                                               |
|                                                                       |
| ▼ ▼ ▼ │                                                               |
|                                                                       |
| Pops ──────► Policy ──► UK ──► World │                                |
|                                                                       |
| │ │                                                                   |
|                                                                       |
| ▼ │                                                                   |
|                                                                       |
| Simulation ───────► Core ◄─────────┘                                  |
|                                                                       |
| │                                                                     |
|                                                                       |
| ▼                                                                     |
|                                                                       |
| Persistence                                                           |
+-----------------------------------------------------------------------+

3.3 Tick Pipeline

The simulation runs on a fixed-step day tick. Default real-time rate is
one day per 2 seconds at game speed 3 of 5.

+-----------------------------------------------------------------------+
| Each day-tick (in this order):                                        |
|                                                                       |
| 1\. Process player input queue (pending decisions, scheme orders)     |
|                                                                       |
| 2\. Tick all active Schemes (+progress, exposure rolls)               |
|                                                                       |
| 3\. Tick all active Events (resolve fired events)                     |
|                                                                       |
| 4\. Tick player Character (stress, age, health)                       |
|                                                                       |
| 5\. Tick Cabinet (loyalty drift)                                      |
|                                                                       |
| 6\. Every 30 ticks (monthly):                                         |
|                                                                       |
| \- Tick Pops (migration, ideology drift)                              |
|                                                                       |
| \- Tick Policies (apply effects)                                      |
|                                                                       |
| \- Tick Situations (+progress, fire stage events)                     |
|                                                                       |
| \- Tick Economy (GDP, deficit, gilts, FX)                             |
|                                                                       |
| \- Tick Polling (recalculate party support)                           |
|                                                                       |
| 7\. Every 365 ticks (annually):                                       |
|                                                                       |
| \- Tick Foreign relations                                             |
|                                                                       |
| \- Newgen pass (generate new politicians)                             |
|                                                                       |
| \- Budget cycle                                                       |
|                                                                       |
| 8\. Render UI based on dirty flags                                    |
|                                                                       |
| 9\. Auto-save every 7 ticks (weekly)                                  |
+-----------------------------------------------------------------------+

3.4 Performance Budget

  ------------------------------------------------------------------------
  **Operation**            **Budget**   **Notes**
  ------------------------ ------------ ----------------------------------
  Daily tick (no UI)       \< 4 ms      Most days only schemes & events
                                        tick

  Monthly tick             \< 40 ms     Pop sim is the bottleneck ---
  (pop+policy+situation)                SQL-backed

  Annual tick (newgen,     \< 200 ms    Can stutter once per year,
  budget)                               acceptable

  UK map render at         60 fps       All 650 polygons batched in
  constituency zoom                     WebGL/Godot

  World map render         60 fps       Topology + flag layer

  Save (full state)        \< 500 ms    Async write; show spinner if \>
                                        300 ms

  Load                     \< 2 s       Cold start from disk
  ------------------------------------------------------------------------

3.5 RNG Determinism

All randomness must use a single seedable RNG instance for
reproducibility. Save files store the current RNG state. AI assistants
implementing this MUST NOT use System.Random directly anywhere in
/Simulation, /Character, /Pops, /Election, or /Narrative.

+-----------------------------------------------------------------------+
| // Required RNG facade in /Core/Rng.cs                                |
|                                                                       |
| public class GameRng {                                                |
|                                                                       |
| private readonly Random \_r;                                          |
|                                                                       |
| public ulong Seed { get; }                                            |
|                                                                       |
| public ulong CallCount { get; private set; }                          |
|                                                                       |
| public GameRng(ulong seed) { Seed = seed; \_r = new                   |
| Random((int)seed); }                                                  |
|                                                                       |
| public int Next(int min, int maxInclusive) { CallCount++; return      |
| \_r.Next(min, maxInclusive + 1); }                                    |
|                                                                       |
| public double NextDouble() { CallCount++; return \_r.NextDouble(); }  |
|                                                                       |
| public bool RollD100Against(int target) { return Next(1, 100) \<=     |
| target; }                                                             |
|                                                                       |
| // serialise CallCount + Seed in save; restore by re-seeding and      |
| advancing                                                             |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4\. Data Models (JSON Schemas)

All persistent game data is JSON-serialisable. C# records map 1:1 to
JSON. Required fields are bold in descriptions; optional fields default
per spec.

4.1 Character

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"char_starmer_keir\", // REQUIRED, unique                    |
|                                                                       |
| \"name\": { \"first\": \"Keir\", \"last\": \"Starmer\",               |
| \"honorific\": \"Sir\" },                                             |
|                                                                       |
| \"birth_date\": \"1962-09-02\", // ISO 8601                           |
|                                                                       |
| \"death_date\": null, // null until dead                              |
|                                                                       |
| \"gender\": \"male\", // male\|female\|nonbinary                      |
|                                                                       |
| \"ethnicity\": \"white_british\",                                     |
|                                                                       |
| \"religion\": \"none\",                                               |
|                                                                       |
| \"sexuality\": \"heterosexual\",                                      |
|                                                                       |
| \"constituency_id\": \"constituency_holborn_and_st_pancras\", //      |
| nullable                                                              |
|                                                                       |
| \"party_id\": \"party_labour\", // nullable                           |
|                                                                       |
| \"career_rank\": 10, // 0..12, see §6.2                               |
|                                                                       |
| \"current_position\": \"pm\", // see §7.3                             |
|                                                                       |
| \"attributes\": { // visible, 1..20, see §6.1                         |
|                                                                       |
| \"charisma\": 9, \"oratory\": 11, \"negotiation\": 16, \"tactics\":   |
| 15, \"vision\": 11,                                                   |
|                                                                       |
| \"intelligence\": 17, \"judgement\": 14, \"composure\": 17,           |
| \"discipline\": 18, \"concentration\": 16,                            |
|                                                                       |
| \"empathy\": 11, \"manipulation\": 12, \"networking\": 13,            |
| \"likeability\": 9, \"authority\": 14,                                |
|                                                                       |
| \"stamina\": 14, \"health\": 15, \"presence\": 11, \"resilience\": 16 |
|                                                                       |
| },                                                                    |
|                                                                       |
| \"hidden\": { // hidden, 1..20, see §6.1                              |
|                                                                       |
| \"loyalty\": 12, \"greed\": 6, \"pragmatism\": 17, \"ambition\": 17,  |
| \"honour\": 14,                                                       |
|                                                                       |
| \"ideology_drift\": 4, \"sociopathy\": 4, \"fear\": 7,                |
| \"risk_appetite\": 8,                                                 |
|                                                                       |
| \"sex_drive\": 8, \"addiction_susceptibility\": 5, \"spirituality\":  |
| 4,                                                                    |
|                                                                       |
| \"coup_plotter_quotient\": 3, \"press_charm\": 11,                    |
| \"working_class_authenticity\": 7,                                    |
|                                                                       |
| \"establishment_acceptance\": 16                                      |
|                                                                       |
| },                                                                    |
|                                                                       |
| \"traits\": \[\"trait_barrister\", \"trait_former_dpp\",              |
| \"trait_cautious\"\],                                                 |
|                                                                       |
| \"ideology_id\": \"ideology_social_democracy\",                       |
|                                                                       |
| \"ideology_purity\": 78, // 0..100                                    |
|                                                                       |
| \"stress\": 32, // 0..100                                             |
|                                                                       |
| \"energy\": 75, // 0..100, refreshed daily                            |
|                                                                       |
| \"money_personal_gbp\": 850000,                                       |
|                                                                       |
| \"relationships\": \[                                                 |
|                                                                       |
| { \"target_id\": \"char_reeves_rachel\", \"value\": 30, \"kind\":     |
| \"ally\" },                                                           |
|                                                                       |
| { \"target_id\": \"char_badenoch_kemi\", \"value\": -20, \"kind\":    |
| \"rival\" }                                                           |
|                                                                       |
| \],                                                                   |
|                                                                       |
| \"hooks_held\": \[\], // see §4.4                                     |
|                                                                       |
| \"hooks_against_me\": \[\],                                           |
|                                                                       |
| \"secrets\": \[\],                                                    |
|                                                                       |
| \"perks_unlocked\": \[\"perk_diplomacy_coalition_builder\"\],         |
|                                                                       |
| \"perk_xp\": { \"diplomacy\": 240, \"martial\": 80, \"stewardship\":  |
| 110, \"intrigue\": 60, \"learning\": 90 },                            |
|                                                                       |
| \"lifestyle_focus\": \"diplomacy\", // current focus tree             |
|                                                                       |
| \"schemes_active\": \[\], // see §4.7                                 |
|                                                                       |
| \"fame\": 12000, // lifetime achievement score                        |
|                                                                       |
| \"is_player\": false,                                                 |
|                                                                       |
| \"spawn_source\": \"seed\" // seed\|newgen\|player_created            |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.2 Constituency

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"constituency_holborn_and_st_pancras\",                      |
|                                                                       |
| \"name\": \"Holborn and St Pancras\",                                 |
|                                                                       |
| \"country\": \"england\", //                                          |
| england\|scotland\|wales\|northern_ireland                            |
|                                                                       |
| \"region\": \"london\", // NUTS-1 region                              |
|                                                                       |
| \"electorate\": 79412,                                                |
|                                                                       |
| \"result_2024\": {                                                    |
|                                                                       |
| \"lab\": 18884, \"grn\": 7312, \"con\": 4282, \"ld\": 2954, \"ref\":  |
| 1700, \"ind_corbyn\": 0, \"other\": 1700                              |
|                                                                       |
| },                                                                    |
|                                                                       |
| \"winner_char_id\": \"char_starmer_keir\",                            |
|                                                                       |
| \"majority\": 11572,                                                  |
|                                                                       |
| \"marginal_class\": \"very_safe\", //                                 |
| very_safe\|safe\|marginal\|key_marginal\|toss_up                      |
|                                                                       |
| \"demographics\": {                                                   |
|                                                                       |
| \"median_age\": 36, \"median_household_income_gbp\": 38000,           |
|                                                                       |
| \"uni_grad_pct\": 0.58, \"white_pct\": 0.59, \"non_uk_born_pct\":     |
| 0.37,                                                                 |
|                                                                       |
| \"owner_occupied_pct\": 0.28, \"social_housing_pct\": 0.24,           |
| \"private_rent_pct\": 0.48                                            |
|                                                                       |
| },                                                                    |
|                                                                       |
| \"topojson_object_id\": \"E14001262\", // refs                        |
| assets/maps/uk_constituencies.topojson                                |
|                                                                       |
| \"lad_id\": \"lad_camden\" // containing local authority              |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.3 Country

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"country_fr\",                                               |
|                                                                       |
| \"iso_a2\": \"FR\", \"iso_a3\": \"FRA\",                              |
|                                                                       |
| \"name_en\": \"France\", \"name_native\": \"France\",                 |
|                                                                       |
| \"head_of_state_id\": \"char_macron_emmanuel\",                       |
|                                                                       |
| \"head_of_government_id\": \"char_lecornu_sebastien\",                |
|                                                                       |
| \"government_type\": \"semi_presidential\",                           |
|                                                                       |
| \"ideology_id\": \"ideology_liberal_centrist\",                       |
|                                                                       |
| \"ruling_parties\": \[\"party_fr_ensemble\"\],                        |
|                                                                       |
| \"gdp_usd_bn_2025\": 3050,                                            |
|                                                                       |
| \"population\": 68500000,                                             |
|                                                                       |
| \"military_score\": 78,                                               |
|                                                                       |
| \"soft_power_score\": 84,                                             |
|                                                                       |
| \"nuclear_armed\": true,                                              |
|                                                                       |
| \"memberships\": \[\"io_un_p5\", \"io_nato\", \"io_eu\", \"io_g7\",   |
| \"io_g20\", \"io_oecd\"\],                                            |
|                                                                       |
| \"relationship_with_uk\": { \"value\": 25, \"treaties\":              |
| \[\"nato\"\], \"trade_volume_gbp_bn\": 78 },                          |
|                                                                       |
| \"topojson_object_id\": \"FRA\",                                      |
|                                                                       |
| \"ai_tier\": 1 // 1 = G20+ (behaviour tree), 2 = utility, 3 =         |
| stochastic                                                            |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.4 Hook & Secret

+-----------------------------------------------------------------------+
| // Hook                                                               |
|                                                                       |
| {                                                                     |
|                                                                       |
| \"id\": \"hook_94821\",                                               |
|                                                                       |
| \"holder_id\": \"char_starmer_keir\",                                 |
|                                                                       |
| \"target_id\": \"char_lammy_david\",                                  |
|                                                                       |
| \"strength\": \"strong\", // weak\|strong                             |
|                                                                       |
| \"source\": \"secret\", // secret\|favour\|leverage                   |
|                                                                       |
| \"source_secret_id\": \"secret_2391\", // nullable                    |
|                                                                       |
| \"expires_date\": null, // strong = null (until exposed); weak = +10y |
| from creation                                                         |
|                                                                       |
| \"used\": false                                                       |
|                                                                       |
| }                                                                     |
|                                                                       |
| // Secret                                                             |
|                                                                       |
| {                                                                     |
|                                                                       |
| \"id\": \"secret_2391\",                                              |
|                                                                       |
| \"subject_id\": \"char_lammy_david\",                                 |
|                                                                       |
| \"knower_ids\": \[\"char_starmer_keir\"\],                            |
|                                                                       |
| \"kind\": \"affair\", //                                              |
| affair\|crime\|corruption\|embarrassing\|ideological                  |
|                                                                       |
| \"severity\": \"shunned\", // shunned (→ weak hook) \| criminal (→    |
| strong hook)                                                          |
|                                                                       |
| \"description\": \"Long-running affair with \[name\]\",               |
|                                                                       |
| \"discovered_date\": \"2024-11-03\",                                  |
|                                                                       |
| \"public\": false                                                     |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.5 Policy Lever

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"policy_income_tax_higher_rate\",                            |
|                                                                       |
| \"name\": \"Higher Rate Income Tax\",                                 |
|                                                                       |
| \"category\": \"fiscal_tax\",                                         |
|                                                                       |
| \"subcategory\": \"income_tax\",                                      |
|                                                                       |
| \"type\": \"slider\", // slider\|toggle\|enum\|bracket\|quota         |
|                                                                       |
| \"min\": 0.0, \"max\": 0.85, \"step\": 0.005,                         |
|                                                                       |
| \"default\": 0.40,                                                    |
|                                                                       |
| \"current_value\": 0.40,                                              |
|                                                                       |
| \"unit\": \"ratio\", // ratio\|gbp\|years\|count\|enum_value          |
|                                                                       |
| \"display_format\": \"percent_1dp\",                                  |
|                                                                       |
| \"dependencies\": \[\],                                               |
|                                                                       |
| \"effects\": \[ // see §8.3 for effect calculus                       |
|                                                                       |
| { \"target\": \"metric.disposable_income.wealthy\", \"fn\":           |
| \"linear\", \"slope\": -800, \"intercept\": 0.40 },                   |
|                                                                       |
| { \"target\": \"metric.gdp_growth\", \"fn\": \"piecewise\", \"knee\": |
| 0.45, \"below_slope\": -0.001, \"above_slope\": -0.020 },             |
|                                                                       |
| { \"target\": \"metric.emigration_rich\", \"fn\": \"exp\", \"k\":     |
| 0.08, \"x0\": 0.45 }                                                  |
|                                                                       |
| \],                                                                   |
|                                                                       |
| \"faction_reactions\": {                                              |
|                                                                       |
| \"faction_city_finance\": -2.0, \"faction_unions\": +0.5,             |
| \"faction_big_business\": -1.2                                        |
|                                                                       |
| },                                                                    |
|                                                                       |
| \"enabled_by_ideologies\": \[\"\*\"\],                                |
|                                                                       |
| \"disabled_by_government_types\": \[\"anarcho_capitalist\"\],         |
|                                                                       |
| \"phase_tag\": \"MVP\"                                                |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.6 Ideology

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"ideology_social_democracy\",                                |
|                                                                       |
| \"name\": \"Social Democracy\",                                       |
|                                                                       |
| \"axis_vector\": {                                                    |
|                                                                       |
| \"economic_lr\": -0.4, \"social_pa\": -0.3, \"environmental_bg\":     |
| +0.4,                                                                 |
|                                                                       |
| \"civic_li\": -0.2, \"foreign_dh\": -0.2, \"monetary_tl\": -0.1,      |
|                                                                       |
| \"constitutional_rr\": -0.1, \"immigration_oc\": -0.3,                |
| \"education_sm\": -0.4,                                               |
|                                                                       |
| \"healthcare_mp\": -0.7, \"justice_rp\": -0.3, \"religion_st\": -0.5  |
|                                                                       |
| },                                                                    |
|                                                                       |
| \"defaults\": {                                                       |
|                                                                       |
| \"policy_income_tax_higher_rate\": 0.45,                              |
|                                                                       |
| \"policy_nhs_spending_pct_gdp\": 0.085,                               |
|                                                                       |
| \"policy_trident_status\": \"replace\",                               |
|                                                                       |
| \"policy_net_zero_target_year\": 2050                                 |
|                                                                       |
| },                                                                    |
|                                                                       |
| \"tolerance_bands\": {                                                |
|                                                                       |
| \"policy_income_tax_higher_rate\": \[0.40, 0.55\],                    |
|                                                                       |
| \"policy_trident_status\": \[\"replace\", \"reduce\"\]                |
|                                                                       |
| },                                                                    |
|                                                                       |
| \"faction_affinity\": {                                               |
|                                                                       |
| \"faction_unions\": +20, \"faction_civil_service\": +5,               |
|                                                                       |
| \"faction_city_finance\": -10, \"faction_press_centre_right\": -15    |
|                                                                       |
| },                                                                    |
|                                                                       |
| \"ui_palette_hex\": \"#E63946\",                                      |
|                                                                       |
| \"true_believer_perks\": \[\"perk_idealist_mobilisation\"\],          |
|                                                                       |
| \"phase_tag\": \"MVP\"                                                |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.7 Scheme

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"scheme_94782\",                                             |
|                                                                       |
| \"template_id\": \"scheme_manufacture_scandal\",                      |
|                                                                       |
| \"owner_id\": \"char_player\",                                        |
|                                                                       |
| \"target_id\": \"char_badenoch_kemi\",                                |
|                                                                       |
| \"progress\": 34, // 0..100                                           |
|                                                                       |
| \"secrecy\": 78, // 0..100                                            |
|                                                                       |
| \"agents\": \[\"char_smith_alex\"\],                                  |
|                                                                       |
| \"started_date\": \"2026-09-12\",                                     |
|                                                                       |
| \"expected_completion_date\": \"2027-01-08\",                         |
|                                                                       |
| \"monthly_progress_gain\": 8,                                         |
|                                                                       |
| \"exposure_chance_monthly\": 0.05,                                    |
|                                                                       |
| \"expected_outcome\": \"target_resigns_or_demoted\",                  |
|                                                                       |
| \"fallback_outcome\": \"no_effect_minor_press\",                      |
|                                                                       |
| \"state\": \"active\" // active\|succeeded\|failed\|exposed\|aborted  |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.8 Situation

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"situation_climate_change\",                                 |
|                                                                       |
| \"scope\": \"global\", // global\|country\|region                     |
|                                                                       |
| \"scope_target\": null, // country_id or region_id, null for global   |
|                                                                       |
| \"progress\": 47, // 0..100                                           |
|                                                                       |
| \"stage\": 2, // 0..N stages                                          |
|                                                                       |
| \"monthly_drift\": +0.12, // baseline drift                           |
|                                                                       |
| \"approach\": \"approach_net_zero_2050\", // currently selected       |
| approach                                                              |
|                                                                       |
| \"approaches_available\": \[\"approach_net_zero_2030\",               |
| \"approach_net_zero_2050\", \"approach_adapt_not_mitigate\",          |
| \"approach_denial\"\],                                                |
|                                                                       |
| \"stage_milestones\": \[                                              |
|                                                                       |
| { \"at\": 25, \"name\": \"Sustained warming trend\", \"fired\": true  |
| },                                                                    |
|                                                                       |
| { \"at\": 50, \"name\": \"Climate events become frequent\",           |
| \"fired\": false },                                                   |
|                                                                       |
| { \"at\": 75, \"name\": \"Tipping points triggered\", \"fired\":      |
| false },                                                              |
|                                                                       |
| { \"at\": 100, \"name\": \"Catastrophic destabilisation\", \"fired\": |
| false }                                                               |
|                                                                       |
| \],                                                                   |
|                                                                       |
| \"contributing_countries\": { \"country_us\": +0.4, \"country_cn\":   |
| +0.6, \"country_uk\": +0.05 }                                         |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.9 Event

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"event_runcorn_byelection_2025\",                            |
|                                                                       |
| \"fired_date\": \"2025-05-01\",                                       |
|                                                                       |
| \"scope\": \"uk\",                                                    |
|                                                                       |
| \"headline\": \"Reform UK win Runcorn & Helsby by-election\",         |
|                                                                       |
| \"category\": \"election_byelection\",                                |
|                                                                       |
| \"decision_required\": false,                                         |
|                                                                       |
| \"options\": \[\], // empty if no decision                            |
|                                                                       |
| \"consequences\": \[                                                  |
|                                                                       |
| { \"target\": \"polling.party_reform\", \"delta\": +1.2 },            |
|                                                                       |
| { \"target\": \"polling.party_labour\", \"delta\": -0.8 },            |
|                                                                       |
| { \"target\": \"situation.political_polarisation.progress\",          |
| \"delta\": +1 }                                                       |
|                                                                       |
| \]                                                                    |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.10 Strategic Initiative (Focus-tree node)

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"initiative_uk_eu_rejoin_path_1\",                           |
|                                                                       |
| \"name\": \"Open the EU Conversation\",                               |
|                                                                       |
| \"tree\": \"tree_uk_strategic_initiatives\",                          |
|                                                                       |
| \"prerequisites\": \[\"initiative_uk_rebuild_trust_with_brussels\"\], |
|                                                                       |
| \"duration_days\": 180,                                               |
|                                                                       |
| \"order_cost_per_year\": 2,                                           |
|                                                                       |
| \"ideology_requirements\": { \"civic_li\": \"\<= 0\" },               |
|                                                                       |
| \"rewards\": \[                                                       |
|                                                                       |
| { \"type\": \"unlock_initiative\", \"value\":                         |
| \"initiative_uk_eea_negotiation\" },                                  |
|                                                                       |
| { \"type\": \"situation_progress\", \"target\":                       |
| \"situation_brexit_aftermath\", \"delta\": -10 }                      |
|                                                                       |
| \],                                                                   |
|                                                                       |
| \"ui_position\": { \"x\": 3, \"y\": 2 },                              |
|                                                                       |
| \"phase_tag\": \"P3\"                                                 |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.11 Faction (Interest Group)

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"faction_unions\",                                           |
|                                                                       |
| \"name\": \"Trade Unions\",                                           |
|                                                                       |
| \"members_pop_count\": 4200000,                                       |
|                                                                       |
| \"approval_of_government\": 38, // 0..100                             |
|                                                                       |
| \"power_score\": 62, // 0..100                                        |
|                                                                       |
| \"ideology_lean\": \"ideology_democratic_socialism\",                 |
|                                                                       |
| \"core_issues\": \[                                                   |
|                                                                       |
| { \"policy_id\": \"policy_workers_rights_strike\",                    |
| \"preferred_value\": \"permissive\", \"weight\": 9 },                 |
|                                                                       |
| { \"policy_id\": \"policy_minimum_wage\", \"preferred_value\": 14.50, |
| \"weight\": 8 }                                                       |
|                                                                       |
| \],                                                                   |
|                                                                       |
| \"leader_id\": \"char_lynch_mick\",                                   |
|                                                                       |
| \"petition_active_id\": null, // see §10.2                            |
|                                                                       |
| \"in_government_party_id\": null // for Vic3-style ruling-faction     |
| mechanics                                                             |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

4.12 Save Game Structure

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"save_version\": 1,                                                  |
|                                                                       |
| \"game_version\": \"0.1.0-mvp\",                                      |
|                                                                       |
| \"game_date\": \"2027-06-15\",                                        |
|                                                                       |
| \"rng_seed\": 8472918374,                                             |
|                                                                       |
| \"rng_call_count\": 12849273,                                         |
|                                                                       |
| \"player_character_id\": \"char_player\",                             |
|                                                                       |
| \"world_state_db\": \"save_01.sqlite\", // SQLite blob alongside the  |
| JSON                                                                  |
|                                                                       |
| \"characters_dirty\": \[\...\], // delta since last full snapshot     |
|                                                                       |
| \"settings\": { \"speed\": 3, \"autopause_events\": true,             |
| \"ironman\": false }                                                  |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

5\. Game Systems --- Core Loop

5.1 SPEC: simulation.tick

  --------------------------------------------------------------------------
  **SPEC:             
  simulation.tick**   
  ------------------- ------------------------------------------------------
  Purpose             Advance the game state by one in-game day.

  Inputs              Current GameState, player input queue, time delta from
                      game speed.

  Outputs             Mutated GameState, list of fired Events, dirty UI
                      flags.

  Dependencies        All other systems (this is the orchestrator).

  Out of scope        Rendering. UI animations. Audio.

  Phase               MVP
  --------------------------------------------------------------------------

Behaviour (pseudocode)

+-----------------------------------------------------------------------+
| function Tick(GameState s, GameRng rng):                              |
|                                                                       |
| s.date = s.date.AddDays(1)                                            |
|                                                                       |
| s.tick_count++                                                        |
|                                                                       |
| foreach scheme in s.schemes_active: TickScheme(scheme, s, rng)        |
|                                                                       |
| foreach event in s.event_queue_today: ResolveEvent(event, s, rng)     |
|                                                                       |
| TickCharacter(s.player, s, rng)                                       |
|                                                                       |
| foreach minister in s.cabinet: TickCharacter(minister, s, rng)        |
|                                                                       |
| if s.tick_count % 30 == 0: TickMonthly(s, rng)                        |
|                                                                       |
| if s.tick_count % 365 == 0: TickAnnual(s, rng)                        |
|                                                                       |
| if s.tick_count % 7 == 0: AutoSave(s)                                 |
|                                                                       |
| MarkUiDirty(s.dirty_flags)                                            |
|                                                                       |
| return s                                                              |
+-----------------------------------------------------------------------+

5.2 SPEC: simulation.speed

  ------------------------------------------------------------------------
  **Speed**    **Real seconds per    **Use case**
               game day**            
  ------------ --------------------- -------------------------------------
  1 (slowest)  10.0                  Reading dense events; new players

  2            5.0                   Cabinet meetings; close decisions

  3 (default)  2.0                   General play

  4            1.0                   Travel between events

  5 (fastest)  0.4                   Waiting out years until election
  ------------------------------------------------------------------------

5.3 SPEC: simulation.autopause

Auto-pause triggers (toggleable per category in settings):

-   Decision event fired (always on by default).

-   Election called.

-   Cabinet minister resigned.

-   Scheme exposed.

-   Character relationship crosses ±50 threshold.

-   Situation crosses a stage milestone.

-   Player character ages by one year.

5.4 SPEC: orders (executive bandwidth currency)

  -----------------------------------------------------------------------
  **SPEC: orders** 
  ---------------- ------------------------------------------------------
  Purpose          Force prioritisation: player can\'t do everything in a
                   month. Old-World-derived.

  Inputs           Player rank, cabinet quality, current government type.

  Outputs          Integer pool refreshed monthly.

  Dependencies     character.career_rank, cabinet quality stat.

  Out of scope     Personal lifestyle actions (those use Energy, see
                   §6.6).

  Phase            MVP
  -----------------------------------------------------------------------

Order pool size by rank

  ----------------------------------------------------------------------------------
  **Rank**   **Title**          **Base           **+ Cabinet      **+ Government
                                orders/month**   bonus**          type**
  ---------- ------------------ ---------------- ---------------- ------------------
  0-3        Citizen--Council   2-4              n/a              n/a
             Leader                                               

  4-5        PPC / MP           5                +0               +0

  6-7        PPS / Jr Minister  6                +1 per loyal     +0
                                                 SpAd             

  8          Cabinet Minister   8                +0..3            +0

  9          Party Leader       10               +0..4            +0
             (opposition)                                         

  10         PM                 14               +0..6            +2 absolute
                                                                  monarchy, --2
                                                                  anarchic
  ----------------------------------------------------------------------------------

Action costs

  -----------------------------------------------------------------------
  **Action**                                 **Order cost**
  ------------------------------------------ ----------------------------
  Start a scheme                             1

  Reshuffle cabinet (small)                  2

  Reshuffle cabinet (major)                  4

  Pass minor policy via SI                   1

  Pass major bill (Vic3-style)               3

  Hold a press conference                    1

  Attend international summit                2

  Reform a Strategic Initiative              1 per year of duration

  Trigger general election                   3

  Cancel an election (dictator move)         8 + legitimacy hit

  Declare war / military intervention        5
  -----------------------------------------------------------------------

5.5 SPEC: time.calendar

Calendar starts at 1 January 2026 (alternative starts: 1 May 2026
post-locals, \'live\' floating start that ingests latest data from
/data/world/refresh.json).

Year length: 365 days (no leap years in MVP). Months: real Gregorian
months. Week starts Monday.

Annual political fixtures fire automatically:

  -----------------------------------------------------------------------
  **Date**       **Event**
  -------------- --------------------------------------------------------
  Late Jan       Bills carried over / King\'s Speech aftermath

  Mid-March      Spring Statement

  1st Thurs in   Local elections (years where applicable)
  May            

  June           Trooping the Colour, garden parties

  Late July      Recess begins

  Sept--Oct      Party conference season (TUC, Lib Dem, Lab, Con, SNP,
                 Reform, Green)

  Early Nov      Budget

  Mid-Dec        Christmas recess
  -----------------------------------------------------------------------

6\. Game Systems --- Character & Rise to Power

6.1 SPEC: character.attributes

  -------------------------------------------------------------------------------
  **SPEC:                  
  character.attributes**   
  ------------------------ ------------------------------------------------------
  Purpose                  Football-Manager-style 1--20 stats driving all
                           character checks.

  Inputs                   Character creation choices, age, trait drift over
                           time.

  Outputs                  Numeric values consumed by all stat-check rolls.

  Dependencies             trait drift, ageing.

  Out of scope             Visual portrait. Voice.

  Phase                    MVP
  -------------------------------------------------------------------------------

Visible attribute groups (19 stats, 1--20)

  -----------------------------------------------------------------------
  **Group**    **Attributes**
  ------------ ----------------------------------------------------------
  Political    Charisma, Oratory, Negotiation, Tactics, Vision

  Mental       Intelligence, Judgement, Composure, Discipline,
               Concentration

  Social       Empathy, Manipulation, Networking, Likeability, Authority

  Physical     Stamina, Health, Presence, Resilience
  -----------------------------------------------------------------------

Hidden attributes (16 stats, 1--20)

Hidden from the player by default. Revealed gradually via Knowledge %
(see §6.3).

+-----------------------------------------------------------------------+
| Loyalty \| Greed \| Pragmatism \| Ambition \| Honour \| Ideology      |
| Drift \| Sociopathy \|                                                |
|                                                                       |
| Fear \| Risk Appetite \| Sex Drive \| Addiction Susceptibility \|     |
| Spirituality \|                                                       |
|                                                                       |
| Coup Plotter Quotient \| Press Charm \| Working Class Authenticity \| |
| Establishment Acceptance                                              |
+-----------------------------------------------------------------------+

Stat-check roll formula

+-----------------------------------------------------------------------+
| // All stat checks use this formula. AI assistants: do not invent     |
| alternatives.                                                         |
|                                                                       |
| function StatCheck(int attribute_value, int difficulty, int           |
| modifiers, GameRng rng):                                              |
|                                                                       |
| // attribute_value: 1..20                                             |
|                                                                       |
| // difficulty: -10 (trivial) .. +10 (nearly impossible)               |
|                                                                       |
| // modifiers: ad-hoc context (e.g., +2 for home turf, -3 for hostile  |
| audience)                                                             |
|                                                                       |
| int target = attribute_value + modifiers - difficulty // typical      |
| range 1..30                                                           |
|                                                                       |
| int roll = rng.Next(1, 20) // d20                                     |
|                                                                       |
| int total = roll + (attribute_value - 10) // FM-style modifier        |
|                                                                       |
| return total \>= (10 + difficulty - modifiers)                        |
+-----------------------------------------------------------------------+

6.2 SPEC: character.career_rank

  --------------------------------------------------------------------------------
  **SPEC:                   
  character.career_rank**   
  ------------------------- ------------------------------------------------------
  Purpose                   12-rank ladder from Citizen (0) to Founder (12). Gates
                            schemes and policies.

  Inputs                    Career events, election wins, appointments.

  Outputs                   Integer 0..12.

  Dependencies              Election system, cabinet system.

  Out of scope              Salary. Lifestyle perks (see §6.4).

  Phase                     MVP
  --------------------------------------------------------------------------------

  -------------------------------------------------------------------------------
  **Rank**   **Title**      **How to reach**             **Unlocks**
  ---------- -------------- ---------------------------- ------------------------
  0          Citizen        Default at character         Personal schemes only
                            creation if started young    

  1          Activist       Join a party, attend events  Organise Rally

  2          Councillor     Win local election           Local policy votes

  3          Council Leader Win council leadership /     Council edicts
             / Mayor        metro mayoralty              

  4          PPC            Win selection contest        Court Donor scheme

  5          MP             Win general election         Parliamentary speech,
                                                         table amendment

  6          PPS            Appointed by minister        Inside-track scheme
                                                         bonus

  7          Junior         Appointed by PM              Sponsor minor bill
             Minister                                    

  8          Cabinet        Appointed by PM              Department control,
             Minister                                    Force Resignation

  9          Party Leader   Win leadership contest       Shadow cabinet, Topple
             (opposition)                                Leader scheme

  10         PM             Win general election as      All policy levers
                            leader                       

  11         Statesman      Retire from PM               Memoir, board seats,
             (post-PM)                                   eminence grise

  12         Founder        Posthumous; legacy carries   Permanent
                            forward                      dynasty/party/network
                                                         bonuses
  -------------------------------------------------------------------------------

6.3 SPEC: character.scouting

  -----------------------------------------------------------------------------
  **SPEC:                
  character.scouting**   
  ---------------------- ------------------------------------------------------
  Purpose                FM-style hidden-attribute reveal. Knowledge % rises
                         with interaction.

  Inputs                 Encounters (meetings, schemes, mentor relationships).

  Outputs                Per-attribute Knowledge % 0..100.

  Dependencies           scheme system, cabinet meetings.

  Out of scope           Scout-network sub-system (P3).

  Phase                  MVP (reveals via direct encounter); P3 (scout network)
  -----------------------------------------------------------------------------

Knowledge growth rules

  -----------------------------------------------------------------------
  **Interaction**                     **Knowledge gain per attribute**
  ----------------------------------- -----------------------------------
  First meeting                       +5

  Worked together on bill             +10 per attribute checked in event

  Cabinet colleague                   +2 per month

  Active scheme target                +15 if scheme successful

  Befriend scheme (CK3-style)         +20 across all hidden attrs on
                                      success

  Intelligence service file unlocked  +50 across all hidden attrs
                                      (one-shot)
  -----------------------------------------------------------------------

Display rule

Below 25 % Knowledge: \'?\'. 25--60 %: range (e.g. \'13--17\'). Above 60
%: exact value with confidence indicator. AI assistants: never reveal
exact hidden values below 60 % Knowledge.

6.4 SPEC: character.lifestyle_perks

  ------------------------------------------------------------------------------------
  **SPEC:                       
  character.lifestyle_perks**   
  ----------------------------- ------------------------------------------------------
  Purpose                       5 lifestyle trees with 25 perks each. Unlocked over a
                                single character\'s lifetime.

  Inputs                        Lifestyle Focus choice (rotatable yearly), XP gained
                                from focus-tagged actions.

  Outputs                       Unlocked perks granting permanent bonuses during
                                character lifetime.

  Dependencies                  character lifecycle.

  Out of scope                  Stat-boosting via training (P2).

  Phase                         P2
  ------------------------------------------------------------------------------------

The five trees (25 perks each --- names abbreviated)

  -----------------------------------------------------------------------
  **Tree**         **Top perks**
  ---------------- ------------------------------------------------------
  Diplomacy /      Coalition-Builder → Whip-Master → Cabinet-Maker →
  Politics         Statesman → Architect of the Settlement

  Martial /        Soldier\'s Friend → Hawk → Securocrat → Wartime Leader
  Security         → Caesar

  Stewardship /    Budget Hawk → Industrialist → Spreadsheet Wizard →
  Economy          Iron Chancellor → Architect of Prosperity

  Intrigue /       Operator → Spinner → Puppet-Master →
  Influence        Manipulator-in-Chief → Eminence Grise

  Learning /       Wonk → Reformer → Visionary → Prophet of the Age →
  Vision           Founder of the New Politics
  -----------------------------------------------------------------------

6.5 SPEC: character.stress_and_coping

  --------------------------------------------------------------------------------------
  **SPEC:                         
  character.stress_and_coping**   
  ------------------------------- ------------------------------------------------------
  Purpose                         CK3-derived. Acting against personality traits
                                  accumulates Stress; thresholds force Coping Mechanism
                                  choice.

  Inputs                          Decisions taken vs trait alignment; defeats;
                                  controversies.

  Outputs                         Stress 0..100; on each break, a chosen Coping trait.

  Dependencies                    trait system, decision event resolution.

  Out of scope                    Therapy unlock tree (P3).

  Phase                           P2
  --------------------------------------------------------------------------------------

Thresholds

  --------------------------------------------------------------------------
  **Stress**   **Effect**
  ------------ -------------------------------------------------------------
  0--24        Composed (default)

  25--49       Tense: -1 to all stat checks

  50--74       Strained: -2 to stat checks; +5% scheme exposure

  75--99       Breaking: -3 to stat checks; +10% exposure; risk of impromptu
               resignation/outburst events

  100          Mental Break: forced coping choice (Drinker / Comfort-Eater /
               Affair / Confider / Burnout-recluse)
  --------------------------------------------------------------------------

6.6 SPEC: character.energy

Energy is a 0..100 daily resource separate from Orders. Lifestyle
actions (drinking with donors, attending charity gala, writing op-ed)
cost Energy. Energy refreshes overnight; refresh capped by Health
attribute. Burnout possible if Energy \< 20 for \>14 consecutive days.

6.7 SPEC: character.ageing

Characters age 1 year per game year (tick_count % 365 == birthday). Age
affects:

-   Below 25: -2 to Authority, +1 to Stamina.

-   25--55: no modifiers.

-   55--70: +2 to Authority (gravitas), -0.05/yr to Stamina and Health.

-   70+: +3 Authority, -0.10/yr to Stamina and Health, +1% death roll
    per year.

-   Trait drift: every 5 years, 1--3 traits may mutate (e.g., +Stubborn,
    +Cynical, --Idealist) weighted by lifetime experience.

6.8 SPEC: character.death

Death rolls per year scale from 0.1 % at age 30 to 50 % at age 95.
Modifiers: -50% if Health 18-20; +200% if Health 1-5; trait Diabetic /
Smoker / etc. add fixed deltas. Other death causes:

-   Assassination --- if active \'Murder\' scheme against player
    succeeds (P3).

-   Coup execution --- if player is deposed by hard coup (P3).

-   Plane crash / accident --- annual 0.05 % baseline, +0.2 % if Risk
    Appetite \>15.

-   Suicide --- only if Stress = 100 and trait depressive (P3+).

6.9 The Rise to Power System (Integrated)

The rise mechanic combines four layers, each spec\'d in its own
subsection:

  ------------------------------------------------------------------------
  **Layer**        **What it does**                       **Spec**
  ---------------- -------------------------------------- ----------------
  L1: Career Track 12-rank ladder, unlocks schemes per    §6.2
                   rank                                   

  L2: Narrative    Suzerain-style hand-authored gates at  §11.6
  Chapters         rank promotions                        

  L3: Faction      N-of-M faction support required per    §10.4
  Endorsements     gate                                   

  L4: Failure      Exile, prison, scandal, assassination  §6.10
  Recovery         --- all continue play                  
  ------------------------------------------------------------------------

6.10 SPEC: character.failure_modes

Failure modes are productive, not game-over. AI assistants: never end
the game on a single character failure unless explicitly requested by
the player via difficulty setting.

  -----------------------------------------------------------------------
  **Mode**        **Effect**                     **Recovery path**
  --------------- ------------------------------ ------------------------
  Exile           Lose seat + 5y banned from UK  Lobby foreign govs, plot
                  politics. Live abroad.         return, write a book,
                                                 return at 6y with renown
                                                 buff

  Prison          Lose seat, lose office.        Serve sentence, release
                  Limited schemes from inside.   with -30 Press Charm but
                                                 +20 Working Class
                                                 Authenticity

  Scandal         -50 Press Charm permanent.     Recovery arc: charity
  Disgrace        Can\'t seek office for 3y.     tour, comeback book,
                                                 rehabilitation chapter
                                                 at 3y

  Assassination   Character dies. Switch to      Continue with successor;
                  designated successor (or party dead character gains
                  leadership contest fires).     martyr status (party
                                                 renown)

  Crash-out       Expelled from party. Sit as    Found new party with
  (party          independent or found new       renown debt, or get
  expulsion)      party.                         elected as independent
                                                 (very hard)
  -----------------------------------------------------------------------

7\. Game Systems --- UK Governance Layer

7.1 SPEC: uk.parliament

  ------------------------------------------------------------------------
  **SPEC:           
  uk.parliament**   
  ----------------- ------------------------------------------------------
  Purpose           Model the 650-seat House of Commons including whip
                    system, free votes, and standing orders.

  Inputs            Election results, defections, by-elections, government
                    bills.

  Outputs           Vote outcomes on every bill; live seat allocation;
                    majority calculations.

  Dependencies      Election system, character system (650 named MPs),
                    party system.

  Out of scope      Detailed standing orders for procedural motions (treat
                    as auto-resolve).

  Phase             MVP
  ------------------------------------------------------------------------

Whip levels

  -----------------------------------------------------------------------
  **Whip**       **Effect on MP vote probability**
  -------------- --------------------------------------------------------
  Free vote (no  MP votes per personal ideology + faction lean
  whip)          

  1-line whip    +5% party-line probability

  2-line whip    +25% party-line probability

  3-line whip    +50% party-line probability; rebellion triggers +Stress,
                 possible withdrawal of whip

  Confidence     +70% party-line; rebellion = lose whip + force
  vote           by-election
  -----------------------------------------------------------------------

Vote resolution algorithm

+-----------------------------------------------------------------------+
| function ResolveCommonsVote(Bill bill, WhipLevel whip, GameState s,   |
| GameRng rng):                                                         |
|                                                                       |
| int ayes = 0, noes = 0                                                |
|                                                                       |
| foreach mp in s.parliament.mps:                                       |
|                                                                       |
| double prob = BaseProbability(mp, bill)                               |
|                                                                       |
| \+ WhipBonus(whip, mp.party_id, bill.govt_position)                   |
|                                                                       |
| \+ IdeologyAlignment(mp.ideology, bill.policy_changes)                |
|                                                                       |
| \+ FactionPressure(mp.faction_lean, bill)                             |
|                                                                       |
| \+ RandomNoise(rng, ±0.10)                                            |
|                                                                       |
| if rng.NextDouble() \< prob: ayes++                                   |
|                                                                       |
| else: noes++                                                          |
|                                                                       |
| return new VoteResult(ayes, noes, RebelMPs(s.parliament.mps, whip))   |
+-----------------------------------------------------------------------+

7.2 SPEC: uk.lords

Model the House of Lords as a body of \~800 peers with no live names
required (a curated 80 named peers cover salient personalities).
Categories: 26 Bishops, \~92 hereditary peers (pending reform), life
peers (\~680).

Lords can:

-   Delay public bills by up to a year (Parliament Acts 1911/1949
    model).

-   Send amendments back to Commons (ping-pong).

-   Block constitutional bills not in manifesto (Salisbury Convention;
    player can break this with consequences).

-   Be reformed: hereditary abolition (active P1 Labour government
    policy), full election, abolition to unicameral, etc.
    (policy_lords_reform_x levers).

7.3 SPEC: uk.cabinet

  -----------------------------------------------------------------------
  **SPEC:          
  uk.cabinet**     
  ---------------- ------------------------------------------------------
  Purpose          Model the \~25-seat Cabinet plus junior ministers
                   (\~115 posts total). Each minister is a named
                   character with portfolio.

  Inputs           PM appointments, resignations, reshuffles.

  Outputs          Department capacity stats, faction reactions to
                   appointments.

  Dependencies     character system, faction system.

  Out of scope     Departmental budget micro-management beyond top-level
                   sliders (handled in §8).

  Phase            MVP
  -----------------------------------------------------------------------

Mandatory Cabinet posts (Phase 1)

These 24 posts must exist at game start with real May 2026 holders. See
§18.1 for the data.

+-----------------------------------------------------------------------+
| Prime Minister · Deputy Prime Minister · Chancellor of the Exchequer  |
|                                                                       |
| Foreign Secretary · Home Secretary · Justice Secretary · Defence      |
| Secretary                                                             |
|                                                                       |
| Health Secretary · Education Secretary · Business & Trade Secretary   |
|                                                                       |
| Work & Pensions Secretary · Environment Secretary · Transport         |
| Secretary                                                             |
|                                                                       |
| Energy Security & Net Zero Secretary · Housing/Communities & LG       |
| Secretary                                                             |
|                                                                       |
| Science Innovation & Tech Secretary · Culture Media & Sport Secretary |
|                                                                       |
| Scotland Secretary · Wales Secretary · Northern Ireland Secretary     |
|                                                                       |
| Leader of House of Commons · Chief Whip · Attorney General ·          |
| Paymaster General                                                     |
+-----------------------------------------------------------------------+

7.4 SPEC: uk.civil_service

17 main departments, named Permanent Secretaries (Antonia Romeo as
Cabinet Sec from Feb 2026 at the top). Each department has Capacity
(0--100) which affects policy implementation effectiveness.

7.5 SPEC: uk.devolved

Three devolved governments modelled:

  -------------------------------------------------------------------------
  **Body**              **Seats**       **Election**     **Current FM (May
                                                         2026)**
  --------------------- --------------- ---------------- ------------------
  Scottish Parliament   129             May 2026 (next   John Swinney (SNP,
  (Holyrood)                            2031)            minority)

  Senedd Cymru (Welsh   96 (new         May 2026 (Plaid  Rhun ap Iorwerth
  Parliament)           proportional)   won 43 seats)    (Plaid) ---
                                                         designate

  NI Assembly           90              Per assembly     Michelle O\'Neill
  (Stormont)                            cycle            (SF) & Emma
                                                         Little-Pengelly
                                                         (DUP) dFM
  -------------------------------------------------------------------------

7.6 SPEC: uk.local_government

Coverage:

-   England: \~317 principal councils (counties, districts, unitaries,
    metropolitan boroughs).

-   Scotland: 32 unitary councils.

-   Wales: 22 principal areas.

-   Northern Ireland: 11 councils.

-   London: 32 London Boroughs + City of London.

-   Metro mayoralties: 12 as of May 2025 (see §18.4).

-   Police & Crime Commissioners: 39 in England + Wales (excluding
    metro-mayor coterminous areas).

7.7 SPEC: uk.crown

Model:

-   Monarch (Charles III) as a named character with full attributes but
    limited active agency.

-   Line of succession array (William, George, Charlotte, Louis, Harry,
    Archie, Lilibet, Andrew, \...).

-   Royal prerogative powers as toggles: Royal Assent withholding,
    dissolution, prorogation, mercy, honours, formal command of armed
    forces.

-   Player as PM can use prerogative via PM-King meeting events.

-   Abolition path available via constitutional reform (see §8
    policies). Triggers a 5-year transition with stability impact.

7.8 SPEC: uk.strategic_initiatives (Focus Tree)

  -----------------------------------------------------------------------------------
  **SPEC:                      
  uk.strategic_initiatives**   
  ---------------------------- ------------------------------------------------------
  Purpose                      HoI4-style focus tree of 5--15-year strategic projects
                               unique to the UK\'s situation.

  Inputs                       Player chooses one Initiative at a time; Order cost
                               per year.

  Outputs                      Locked-in changes when complete (policy preset jumps,
                               situation deltas, unlocks).

  Dependencies                 Order system, policy system, situation system.

  Out of scope                 Generic \'research\' trees (those are policy-driven).

  Phase                        P3
  -----------------------------------------------------------------------------------

Seed UK Initiative trees

  -----------------------------------------------------------------------
  **Tree**         **Mutually exclusive branches**
  ---------------- ------------------------------------------------------
  UK--EU Relations Rejoin Path · EEA Path · CPTPP Doubledown · Sovereign
                   Trade · Customs Union Lite

  Constitution     Federalise UK · Codify Constitution · Abolish Lords ·
                   Restore Crown Powers · Republic by Referendum

  Economy          Tech Sovereignty Sprint · Reindustrialise the North ·
                   Financial Centre+ · Green New Deal · Wage-Led Growth

  Defence          Trident Plus · Trident Replacement Only · Nuclear
                   Disarmament · Conventional Hardening · European Pillar

  Devolution       Indyref2 · Welsh Indyref · Irish Border Poll · English
                   Parliament · Status Quo Plus

  Climate          Net Zero 2035 · Net Zero 2045 · Net Zero 2055 ·
                   Adapt-not-Mitigate · Climate Denial
  -----------------------------------------------------------------------

8\. Game Systems --- Policy Engine

8.1 SPEC: policy.lever

  -----------------------------------------------------------------------
  **SPEC:          
  policy.lever**   
  ---------------- ------------------------------------------------------
  Purpose          Single atomic unit of governable policy. Player
                   adjusts; simulation responds.

  Inputs           Player slider/toggle changes.

  Outputs          Effect deltas applied to metrics, pops, factions,
                   situations.

  Dependencies     metric system, pop system, faction system.

  Out of scope     Policy bundling into \'manifesto promises\' (P3).

  Phase            MVP (50 levers) → P2 (200) → P5 (525)
  -----------------------------------------------------------------------

Data schema in §4.5. AI assistants implementing levers MUST keep all
effect computations pure (no I/O, no random draw --- randomness applied
separately by situation system).

8.2 SPEC: policy.enactment (Vic3-style three-phase)

Major policy changes require enactment, not just a slider drag.
Enactment phases (Vic3 1.3-derived):

  -----------------------------------------------------------------------
  **Phase**    **Trigger**         **Outcome on roll**
  ------------ ------------------- --------------------------------------
  1\. Drafting Player selects new  Success → Advance to Debate; Stall →
               policy value        restart next month

  2\. Debate   Bill on floor of    Success → Advance to Royal Assent;
               Commons             Stall → return to Drafting; Fail →
                                   killed

  3\. Royal    Bill passes both    Auto-success in democracy; Lords
  Assent       houses              ping-pong can delay; King can withhold
                                   (extreme)
  -----------------------------------------------------------------------

Which policies need enactment vs. instant change

  ------------------------------------------------------------------------
  **Category**                   **Instant    **Enactment required**
                                 change**     
  ------------------------------ ------------ ----------------------------
  Department budget reallocation ✓            
  (within envelope)                           

  Tax rate within ±2pp of        ✓            
  current                                     

  Tax rate change beyond ±2pp,                ✓
  new tax                                     

  Welfare benefit rate           ✓ (statutory 
                                 order)       

  Major social law (abortion,                 ✓
  drugs, marriage)                            

  Electoral system change                     ✓✓ (referendum required for
                                              some)

  Constitutional change (Lords,               ✓✓✓
  monarchy)                                   (supermajority/referendum)

  Foreign treaty (FTA, NATO, EU)              ✓ (CRAG Act process)
  ------------------------------------------------------------------------

8.3 SPEC: policy.effect_calculus

Every lever declares one or more effects. Effect functions (pure):

+-----------------------------------------------------------------------+
| linear: y = slope \* (x - intercept)                                  |
|                                                                       |
| piecewise: y = below_slope\*(x - knee) for x\<knee else               |
| above_slope\*(x - knee)                                               |
|                                                                       |
| exp: y = k \* exp(x - x0)                                             |
|                                                                       |
| log: y = k \* log(1 + (x - x0))                                       |
|                                                                       |
| step: y = value_below if x\<threshold else value_above                |
+-----------------------------------------------------------------------+

Effects fan into typed targets:

+-----------------------------------------------------------------------+
| metric.gdp_growth (national)                                          |
|                                                                       |
| metric.disposable_income.{stratum} (per stratum)                      |
|                                                                       |
| metric.deficit_pct_gdp                                                |
|                                                                       |
| metric.unemployment                                                   |
|                                                                       |
| metric.inflation                                                      |
|                                                                       |
| pop.ideology_drift.{ideology_id} (per ideology axis)                  |
|                                                                       |
| pop.migration_in / pop.migration_out                                  |
|                                                                       |
| faction.{faction_id}.approval                                         |
|                                                                       |
| situation.{situation_id}.progress                                     |
|                                                                       |
| party.{party_id}.polling                                              |
+-----------------------------------------------------------------------+

8.4 SPEC: policy.faction_reactions

Each lever declares faction_reactions: a map of faction_id → delta. On
every monthly tick, each affected faction\'s approval moves toward the
delta-weighted target. AI assistants: faction reactions are linear and
additive --- do not invent nonlinear escalation.

8.5 SPEC: policy.ideology_compatibility

Levers are gated by:

-   enabled_by_ideologies: list of ideology IDs that can use this
    policy. \'\*\' wildcard = all.

-   disabled_by_government_types: e.g. wealth tax disabled under
    anarcho_capitalist; censorship disabled under classical_liberal.

-   ideology_purity_cost: how much Purity Loss the player accumulates by
    deviating from their stated ideology (see §9.4).

8.6 The 525-Lever Catalogue (Headlines)

Full enumeration below. Each category lists count and key representative
levers. Full list in /content/policies/ as JSON.

Fiscal --- Taxation (\~80 levers)

Income tax: personal allowance, basic/higher/additional rate thresholds
and rates, marriage allowance, blind person\'s allowance, savings
allowance, dividend allowance, dividend rates × 3, Scottish bands × 5,
Welsh rates × 3. NICs: Class 1 employee/employer/1A/2/4 with thresholds.
Corporation Tax: main, small profits, marginal relief, R&D RDEC + SME
rates. VAT: standard, reduced, zero-rated line items × 6, registration
threshold. CGT: basic, higher, residential surcharge, BADR, investors\'
relief. IHT: nil-rate band, residence NRB, rate, taper, business relief,
agri relief. SDLT: 5 bands + non-resident surcharge + additional
dwellings. Council Tax bands A--H, capping. Business Rates: multiplier,
small business, retail/hospitality discount, transitional. Fuel duty × 6
fuel types. Alcohol duty per ABV strata. Tobacco specific + ad valorem,
HRT, RYO. Sugar Levy bands. APD × 3 bands × short/long/private. VED
bands. Gambling × 4 types. Bank levy. Bank surcharge. IPT. CCL.
Aggregates. Landfill. Plastic packaging. DST. Optional: Wealth Tax, LVT,
FTT, Windfall, CBAM.

Fiscal --- Spending (\~90 levers)

Per department envelope + sub-programmes: NHS (RDEL, capital DEL, GP,
hospitals, mental health, public health, social care, dentistry, primary
care, pharma, NHS pay). Education (per-pupil, sixth form, FE, HE
teaching grant, student loans, apprenticeship levy, EHCP). Defence (%
GDP, Army/Navy/RAF, Strategic Command, Trident running, replacement).
Welfare (UC standard, taper, work allowances, housing element, sanctions
× 5 severity, PIP, Carer\'s, Pension Credit, State Pension triple lock &
rate). Pensions (SPA years, triple lock components). Transport (rail per
franchise, road per region, HS2 status, active travel, EV grants).
Housing (affordable, social rent, RTB, leasehold, planning fees). Energy
(CfD strike prices per tech, ECO, WHD). Foreign aid (% GNI, bilateral vs
multilateral).

Monetary & Currency (\~25 levers)

BoE mandate (single/dual/NGDP/PLT), inflation target 0--5%, corridor
width, QE/QT stance, bank rate (player-set under End-BoE-Independence),
MPC composition. Currency regime: free float / managed / peg USD / peg
EUR / euro adoption / gold / sterling-crypto hybrid. Capital controls.
Macroprudential: LTV cap, LTI cap, CCyB.

Trade & Customs (\~30 levers)

WTO baseline. FTAs: UK-EU TCA depth, UK-US, UK-CPTPP (member since Dec
2024), UK-India, UK-GCC, UK-Mercosur, UK-China. Customs Union toggle.
Single Market re-entry. Windsor Framework parameters. Tariffs per HS
chapter (96 simplified to \~20 sectors). Anti-dumping. Export controls.
Sanctions management.

Social Policy (\~50 levers)

Abortion (24wk + 5 conditions). Drug policy per substance class.
Psychedelics. All-drugs Portugal model toggle. Assisted dying
(terminal-only / mental-suffering / age limits). Marriage equality.
Polygamy. GRA parameters (self-ID / panel / age / waiting). Surrogacy
(altruistic / commercial / international). Prostitution (criminalised /
Nordic / decrim / legalised regulated). Gambling (advertising, FOBT
stake, online affordability). Hunting Act. Fox hunting. Animal welfare
(livestock, lab testing, exotic pets). Smoking ban age.

Education (\~40 levers)

School system (comprehensive / grammar+modern / academy / free /
Steiner). Curriculum (NC on/off, RE compulsory, RSE content, history
empire/decolonial). Faith schools. Tuition fees £0--£15k. Maintenance
grants vs loans. Apprenticeship levy %. T-Levels & BTECs status. Class
size cap. Teacher pay scales. Ofsted regime.

Healthcare (\~35 levers)

Structure: Beveridge (status quo) / Bismarck / Singapore / Hybrid / Full
private. Co-pays (GP, prescription, A&E). Drug pricing NICE QALY
threshold. Mental health parity. Public health (sugar, salt, alcohol
unit pricing). Vaccine schedule. Abortion access on NHS. Gender clinics
policy. Social care funding (Dilnot cap / free / means-tested).

Justice (\~40 levers)

Sentencing (mandatory minimums per offence class, three-strikes, death
penalty toggle with sub-options). Policing (officer numbers per force,
stop & search authorisations, taser issue, routine armed). Prisons
(privatisation %, ratio, education hours, conditions). Drugs (possession
× substance, supply × substance). Rehabilitation--punishment slider.
Restorative justice.

Immigration (\~30 levers)

Each visa category (Skilled Worker, Health & Care, Student, Graduate,
Family, Investor, Innovator, Global Talent, Seasonal, Youth Mobility)
--- salary threshold, cap, route on/off. Asylum: Dublin returns,
safe-third list, processing speed, accommodation type, work rights after
N months, deportation target. Channel crossings (deterrence package,
returns, France co-op). Citizenship (residency years, language level,
civic test, ceremony, dual nationality, oath wording). Birthright.
Integration (English funding, \'British values\' mandate).

Civil Liberties (\~30 levers)

Surveillance (IPA bulk × type, equipment interference, warrant
sign-off). Free speech (Online Safety Act scope, hate speech statutory
definition, blasphemy revival, hate crime per protected characteristic).
Protest (Public Order Act thresholds, lock-on, slow-walk, noise
threshold). Encryption (back doors, E2E legal). ID cards (none /
voluntary / mandatory + db scope). Biometrics (FR by police, retention,
children). Data retention duration.

Constitutional (\~50 levers)

Electoral system (FPTP / AV / SV / STV / MMP / list-PR open/closed / AMS
/ Borda). Boundaries authority. Voting age 16/18. Prisoners\' vote.
Expat lifetime. Compulsory voting. Voter ID. House of Lords (status quo
/ appointed only / fully elected / hybrid / abolish). Bishops\' seats.
Hereditaries. Monarchy (status quo / abolish / referendum / restore
absolute). Devolution (status quo / federalism / English Parliament /
regional assemblies / abolish / IndyRef2 grant). Welsh Indyref. NI
border poll. Judicial review (preserve / curtail / abolish). Supreme
Court status. Codified Constitution. Bill of Rights replacing HRA. ECHR
withdrawal.

Foreign Policy & Defence (\~50 levers)

NATO (member / leave / nuclear umbrella renegotiate). EU (TCA / EEA / CU
/ full re-entry). Five Eyes (preserve / loosen / expand). AUKUS (deepen
/ withdraw). Commonwealth role. UN posture. UNSC reform. Recognition
toggles: Palestine / Taiwan / Western Sahara / Kosovo / N Cyprus / S
Ossetia / Abkhazia / Somaliland / Transnistria. Trident (full
replacement / reduce / abolish / expand). CASD on/off. Army (per
regiment). Navy hull count (CV × 2, T26, T31, T45, SSN, SSBN). RAF
squadrons. Space Command. Conscription (none / selective / universal /
civilian). Defence % GDP. Foreign bases (Cyprus, BIOT/Diego Garcia,
Gibraltar, Falklands, BFG, Belize, Brunei, Oman, Bahrain).

Environment (\~30 levers)

Net Zero year (2030 / 2040 / 2050 / abolish). Sector pathways × 6
(electricity, transport, buildings, industry, agriculture, aviation).
Carbon price (UK ETS, CBAM). Fracking (banned / regional / national).
North Sea (max licences / managed decline / new / total ban). Nuclear
(SMR programme, Hinkley/Sizewell, fast-track planning). Renewables mix
targets. Planning reform (NSIP fast-track). Tree planting. Peatland. CCS
deployment.

Industrial, Regional, AI/Digital (\~30 levers)

Industrial strategy 8 priority sectors + financial top-up. R&D % GDP
target. Regional Growth funds. Levelling Up successor. Competition
policy stance. State ownership per industry (rail, water, Royal Mail,
ports, energy retail, steel, defence primes). AI regulation (UK
Bletchley / EU AI Act-style / laissez-faire). GDPR (retain / reform /
abolish). Online platform regulation. Crypto (CBDC pilot, retail status,
mining ban). Encryption back door (linked civil liberties).

Devolution-Specific (\~15 levers)

IndyRef2 (grant / refuse / threshold-based). Section 35 use. Devolve
immigration to Scotland. Devolve full income tax to Wales. English
Devolution & Community Empowerment Bill scope. Council of Nations &
Regions formal powers. Reform NI petition-of-concern thresholds.

Total

525 distinct levers before counting per-region/per-sector duplications.
MVP ships with the first 50 (listed in §8.7).

8.7 The MVP 50 Levers (Phase 1 scope)

Implement these in MVP. All other levers stubbed as visible-but-disabled
in UI.

  ----------------------------------------------------------------------------
  **\#**   **Lever**                                          **Category**
  -------- -------------------------------------------------- ----------------
  1-5      Income tax: personal allowance, basic rate, higher Fiscal --- Tax
           threshold, higher rate, additional rate            

  6-8      VAT: standard rate, reduced rate, registration     Fiscal --- Tax
           threshold                                          

  9-10     Corporation tax main rate, small profits rate      Fiscal --- Tax

  11-12    NICs Class 1 employee, Class 1 employer            Fiscal --- Tax

  13       Council Tax cap %                                  Fiscal --- Tax

  14-15    Capital Gains Tax basic, higher                    Fiscal --- Tax

  16-20    Department spending (% GDP): NHS, Education,       Fiscal --- Spend
           Defence, Welfare, Transport                        

  21       State Pension triple lock toggle                   Fiscal --- Spend

  22-23    UC standard allowance, taper rate                  Welfare

  24       Minimum wage £/hr                                  Labour

  25       Net Zero target year (2030/2040/2050/abolish)      Environment

  26       Carbon price £/tCO2                                Environment

  27       Defence % GDP                                      Defence

  28       Trident status (full replacement / reduce /        Defence
           abolish)                                           

  29       NATO membership (stay / leave)                     Foreign

  30       Foreign aid % GNI                                  Foreign

  31-32    Asylum processing speed, deportation target        Immigration

  33-34    Skilled Worker visa cap, salary threshold          Immigration

  35       Channel crossings deterrence package level         Immigration

  36       Tuition fees £                                     Education

  37       NHS GP funding %                                   Health

  38       Drugs: cannabis (prohibition / decrim / regulated) Social

  39       Assisted dying (none / terminal only / broader)    Social

  40       Voting age (16 / 18)                               Constitutional

  41       House of Lords reform (status quo / abolish        Constitutional
           hereditaries / fully elected / abolish)            

  42       Electoral system (FPTP / AV / STV / list-PR)       Constitutional

  43       IndyRef2 policy (refuse / grant)                   Devolution

  44-45    Bank Rate, Inflation target (Bank Rate only if BoE Monetary
           independence ended)                                

  46       BoE independence (preserve / end)                  Monetary

  47       EU relationship (TCA / EEA / CU / re-entry)        Foreign

  48       Policing: stop & search authorisation level        Justice

  49       Prisons privatisation %                            Justice

  50       Online Safety Act scope (narrow / current /        Civil Liberties
           expanded / repeal)                                 
  ----------------------------------------------------------------------------

9\. Game Systems --- Ideology & Government Type

9.1 SPEC: ideology

An ideology is a 12-axis vector + policy defaults + tolerance bands +
faction affinity + cosmetic. Schema in §4.6. Player can pick from 30+
presets or build custom.

9.2 The 30+ Named Ideologies

All must be implementable by P3. MVP ships with 10 (marked \*).

  -----------------------------------------------------------------------
  **Family**     **Ideologies**
  -------------- --------------------------------------------------------
  Democratic     Social Democracy\* · Democratic Socialism ·
  Left           Eco-Socialism · Green Liberalism · Distributism

  Far Left       Marxist-Leninist Communism\* · Trotskyism ·
                 Anarcho-Syndicalism · Anarcho-Communism ·
                 Council-Communism

  Liberal Centre Classical Liberalism · Modern Liberalism\* ·
                 Ordoliberalism · Christian Democracy · Technocracy\*

  Libertarian    Libertarianism\* · Minarchism · Anarcho-Capitalism ·
                 Georgism · Mutualism

  Conservative   One-Nation Conservatism\* · Thatcherite Conservatism\* ·
                 National Conservatism · Bonapartism · Corporatism

  Far Right      Fascism\* · Falangism · Integralism · Accelerationism

  Religious      Theocracy (Christian / Islamic / Hindu / Jewish variants
                 --- 4 sub-IDs)

  Monarchist     Monarchist Restoration

  Futurist       Transhumanist Technocracy · Bioconservatism · Reform
                 Populism\*
  -----------------------------------------------------------------------

9.3 The 12-Axis Custom Ideology Builder

Player allocates 100 points across 12 axes. Each axis runs -1.0 to +1.0.
Tooltip describes endpoints. AI generates a flag and name from the
vector.

  -----------------------------------------------------------------------------
  **Axis**            **−1.0 endpoint**            **+1.0 endpoint**
  ------------------- ---------------------------- ----------------------------
  economic_lr         Command economy              Pure free market

  social_pa           Permissive                   Authoritarian

  environmental_bg    Growth-first brown           Deep ecology

  civic_li            Cosmopolitan liberal         Identitarian-nationalist

  foreign_dh          Dovish / pacifist            Hawkish / interventionist

  monetary_tl         Tight / hard money           Loose / MMT

  constitutional_rr   Reformist                    Reactionary status quo

  immigration_oc      Open borders                 Closed borders

  education_sm        Comprehensive secular        Selective + religious

  healthcare_mp       Single-payer public          Fully private insurance

  justice_rp          Rehabilitative               Punitive

  religion_st         Strictly secular             State-religion fusion
  -----------------------------------------------------------------------------

9.4 SPEC: ideology.purity_drift

Holding power against your ideology\'s tolerance band ticks +Purity
Loss. Reaching 100 Purity Loss triggers a faction revolt event or a
personal Crisis-of-Faith narrative chapter (P3).

+-----------------------------------------------------------------------+
| function PurityCheck(Ideology id, Dictionary\<PolicyId, Value\>       |
| current, Character pc):                                               |
|                                                                       |
| int loss = 0                                                          |
|                                                                       |
| foreach (policyId, defaultValue) in id.defaults:                      |
|                                                                       |
| var actualValue = current\[policyId\]                                 |
|                                                                       |
| var band = id.tolerance_bands.GetOrDefault(policyId, ±5%)             |
|                                                                       |
| if !InBand(actualValue, defaultValue, band):                          |
|                                                                       |
| loss += DeviationCost(actualValue, defaultValue, band)                |
|                                                                       |
| pc.ideology_purity = Clamp(100 - loss, 0, 100)                        |
+-----------------------------------------------------------------------+

9.5 SPEC: government_type

15 government types modelled. Each enables/disables policy clusters.

  ---------------------------------------------------------------------------------
  **Government type**                   **Enables**           **Disables**
  ------------------------------------- --------------------- ---------------------
  Parliamentary Constitutional Monarchy Standard UK policies  Restored absolute
  (default)                                                   monarch powers

  Parliamentary Republic                Elected head of state Hereditary monarch
                                                              role

  Presidential Republic                 Strong executive      PM as head of govt

  Semi-Presidential                     Both Pres + PM        Pure Westminster mode

  Dominant-Party Democracy              Soft authoritarian    Opposition victory
                                                              probability reduced

  One-Party State                       Single ruling party   Multi-party elections

  Military Junta                        Direct military rule  Civilian electoral
                                                              path

  Theocracy                             State religion        Anti-religious
  (CofE/Catholic/Islamic/Hindu/Pagan)   policies              policies

  Absolute Monarchy (restored)          Royal prerogative     Most parliamentary
                                        wholesale             policies

  Anarcho-Capitalist Minimal State      Privatise everything  Most public spending
                                        menu                  

  Technocracy                           Expert councils       Populist appeal
                                        replace ministers     mechanic

  Soviet / Council Democracy            Workplace councils    Capitalist property

  Direct Democracy                      Referendum-heavy      Cabinet autonomy

  Corporate State                       Chambers by industry  Class-based
                                                              representation

  Confederation                         Devo-max federation   Centralised UK
                                                              government
  ---------------------------------------------------------------------------------

9.6 SPEC: government_type.transition

Each transition is a multi-step Strategic Initiative. Requirements:

-   Legitimacy threshold (varies by transition severity).

-   Faction endorsements (N of M; revolutionary changes need
    supermajority of factions).

-   Constitutional vote / coup d\'état / referendum (mechanism depends
    on transition).

-   Settling period: 3--10 game years with elevated instability and
    counter-coup chance.

10\. Game Systems --- Pops, Factions & Elections

10.1 SPEC: pops

  -----------------------------------------------------------------------
  **SPEC: pops**   
  ---------------- ------------------------------------------------------
  Purpose          Vic3-derived pop simulation modelling the UK
                   population at policy-responsive granularity.

  Inputs           Policy changes, situations, narrative events,
                   migration flows.

  Outputs          Pop counts per stratum/region/ideology. Drives voting,
                   faction power, economic stats.

  Dependencies     policy effect calculus, faction system.

  Out of scope     Individual pop micro-management. Detailed birth/death
                   tracking.

  Phase            MVP (1,000 pops × 12 regions) → P2 (6,000 pops × 650
                   constituencies)
  -----------------------------------------------------------------------

Pop schema (compact)

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"pop_010432\",                                               |
|                                                                       |
| \"region_id\": \"region_london\", // or constituency_id at P2         |
|                                                                       |
| \"size\": 47120, // population represented                            |
|                                                                       |
| \"stratum\": \"middle\", // poor\|middle\|wealthy                     |
|                                                                       |
| \"profession\": \"professional_services\",                            |
|                                                                       |
| \"ideology_vector\": { economic_lr: +0.2, social_pa: -0.4, \... },    |
|                                                                       |
| \"ethnicity\": \"white_british\",                                     |
|                                                                       |
| \"religion\": \"none\",                                               |
|                                                                       |
| \"age_cohort\": \"25-44\",                                            |
|                                                                       |
| \"education\": \"degree\",                                            |
|                                                                       |
| \"engagement\": 0.62 // 0..1, propensity to vote                      |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

Pop migration & drift rules

  -----------------------------------------------------------------------
  **Trigger**               **Effect**
  ------------------------- ---------------------------------------------
  Policy raises disposable  X% of S-stratum pops move up a stratum over Y
  income for stratum S      months

  Region GDP growth         Pop migration in/out at rate proportional to
  diverges from neighbours  delta

  Media narrative pings     Pops within ±0.2 of pin drift toward
  ideology axis             narrative pole by 0.01/month

  Generational replacement  Annual rollover: oldest cohort declines,
                            youngest grows; new cohort ideology drawn
                            from current parental + media climate
  -----------------------------------------------------------------------

10.2 SPEC: factions

13 strategic factions modelled at MVP. Schema in §4.11.

  -----------------------------------------------------------------------
  **Faction**             **Power      **Core preference**
                          score        
                          baseline**   
  ----------------------- ------------ ----------------------------------
  Big Business (FTSE 100  62           Low corp tax, light regulation
  lobby)                               

  City of London Finance  70           Sterling stability, no FTT, light
                                       banking regulation

  Trade Unions Congress   55           High public sector pay, strong
                                       workers\' rights

  Civil Service mandarins 60           Capacity, continuity, gradualism

  Military Top Brass      58           2.5%+ GDP defence, Trident
                                       replacement, NATO

  Intelligence Community  62           Surveillance powers, secrecy
                                       preserved

  Monarchy (informal)     45           Status quo, royal prerogative
                                       preserved

  Church of England       35           Bishops\' bench, faith schools,
                                       traditional marriage

  Free Churches &         25           Abortion restriction, faith
  Evangelicals                         schools

  British Muslims         40           Religious freedom,
  (representative bodies)              anti-Islamophobia, recognition of
                                       Palestine

  British Jews (Board of  35           Anti-antisemitism, Israel support
  Deputies)                            

  Tech Sector & Tech      55           Light AI regulation, immigration
  Billionaires                         for talent

  Media Barons (Murdoch / 65           Press freedom, no statutory
  Rothermere / Lebedev /               regulation
  Reach / BBC)                         
  -----------------------------------------------------------------------

10.3 SPEC: faction.petition

Each faction in government may submit a Petition (Vic3 1.3 mechanic).
Petitions:

-   Fire once per year per faction that is part of the government
    coalition.

-   Demand a specific policy change.

-   Granting it: +20 approval with that faction, possibly -15 with
    opposed faction.

-   Denying: -10 approval with that faction, growing risk of withdrawing
    support / defection.

10.4 SPEC: faction.endorsements

Career rank gates require faction endorsements. Per-rank requirements:

  ---------------------------------------------------------------------------
  **Rank goal**    **Endorsements   **Faction pool**
                   needed**         
  ---------------- ---------------- -----------------------------------------
  Become PPC       1 of 3           Local party, donor, union (if Labour) or
                                    business (if Con)

  Become MP        2 of 4           Local party, regional press, key local
                   (informal)       faction, polling momentum

  Become Cabinet   2 of 6           Party leader\'s circle, party right/left,
  Minister                          donors, factional ally MPs

  Become Party     3 of 8           Backbench MPs, donor base,
  Leader                            unions/business, regional barons, members

  Become PM        4 of 10 +        Above plus broader civil society +
                   electoral        press + key marginals
                   mandate          
  ---------------------------------------------------------------------------

10.5 SPEC: elections.fptp

  -------------------------------------------------------------------------
  **SPEC:            
  elections.fptp**   
  ------------------ ------------------------------------------------------
  Purpose            Simulate a UK general election under
                     First-Past-The-Post. Default electoral system.

  Inputs             Current polling, constituency demographics, incumbent
                     strength, manifestos, campaign spend.

  Outputs            650 individual seat results, aggregate party seat
                     count, government formation outcome.

  Dependencies       Party system, pop system, polling system, character
                     system.

  Out of scope       Detailed per-constituency canvassing micro-game (P3).

  Phase              MVP
  -------------------------------------------------------------------------

Seat-by-seat algorithm

+-----------------------------------------------------------------------+
| function ResolveFptpElection(GameState s, GameRng rng):               |
|                                                                       |
| Dictionary\<PartyId, int\> seats = new()                              |
|                                                                       |
| foreach c in s.uk.constituencies:                                     |
|                                                                       |
| // baseline: 2024 result vote shares                                  |
|                                                                       |
| var shares = c.result_2024.SharesNormalised()                         |
|                                                                       |
| // apply national swing per party                                     |
|                                                                       |
| foreach (party, share) in shares.ToList():                            |
|                                                                       |
| double swing = NationalSwing(party, s) - 1.0 // delta from baseline   |
|                                                                       |
| shares\[party\] \*= (1.0 + swing)                                     |
|                                                                       |
| // apply local effects: incumbent ±2pp, demographic alignment,        |
| scandals, by-election bump                                            |
|                                                                       |
| ApplyLocalEffects(c, shares, s, rng)                                  |
|                                                                       |
| // normalise to 1.0                                                   |
|                                                                       |
| Normalise(shares)                                                     |
|                                                                       |
| // add stochastic noise per party ±3pp                                |
|                                                                       |
| AddNoise(shares, rng, sigma: 0.03)                                    |
|                                                                       |
| // winner = max share; tiebreaker by Coin Flip then alphabetical      |
|                                                                       |
| var winner = shares.OrderByDescending(kv =\> kv.Value).First().Key    |
|                                                                       |
| c.winner_party_id = winner                                            |
|                                                                       |
| seats\[winner\]++                                                     |
|                                                                       |
| return new ElectionResult(seats, s.date)                              |
+-----------------------------------------------------------------------+

10.6 SPEC: elections.alternative_systems

If the player has enacted an alternative system via constitutional
reform, use the appropriate algorithm:

  ------------------------------------------------------------------------
  **System**        **Algorithm**
  ----------------- ------------------------------------------------------
  AV (Alternative   Per-constituency 1st-pref count; eliminate last,
  Vote)             redistribute 2nd prefs, repeat until majority

  STV (Single       Multi-member constituencies (Phase 3 redraws); Droop
  Transferable      quota; transfer surplus + eliminated
  Vote)             

  MMP (Mixed-Member Half FPTP, half top-up list seats; D\'Hondt for
  Proportional)     top-ups

  List PR           National or regional lists; D\'Hondt or Sainte-Laguë
  (open/closed)     

  AMS (current      Constituency FPTP + regional list top-up
  Holyrood/Senedd   
  style)            
  ------------------------------------------------------------------------

10.7 SPEC: elections.leadership_contest

Party leadership election as a multi-round mini-game (Tropico-derived):

-   Round 1 --- Nominations gate (need N MPs to nominate; varies by
    party rule).

-   Round 2 --- MP ballot (multi-round; eliminate last each time until 2
    remain). Player makes pitch speeches with stat checks.

-   Round 3 --- Membership ballot (if applicable; Labour, Conservative,
    Lib Dem). Manifesto policies become Strategic Initiatives if won.

-   Round 4 --- Acceptance speech narrative chapter.

10.8 SPEC: elections.local

Local elections fire annually in May where applicable. Each council seat
resolves with simplified shares (ward-level demographic + national swing
× 0.7). Output: seat changes per council; metro mayor outcomes; PCC
outcomes.

11\. Game Systems --- Schemes, Cabinet & Narrative

11.1 SPEC: scheme

  -----------------------------------------------------------------------
  **SPEC: scheme** 
  ---------------- ------------------------------------------------------
  Purpose          CK3-derived multi-tick covert action. Drives intrigue
                   gameplay.

  Inputs           Template choice, target, agent assignments, Order
                   spend.

  Outputs          Resolved outcome (success/failure/exposed) and
                   consequences.

  Dependencies     Character system, hook/secret system.

  Out of scope     Auto-scheme by AI characters (P2). For now, only the
                   player initiates schemes.

  Phase            MVP (5 schemes) → P3 (25 schemes)
  -----------------------------------------------------------------------

MVP 5 schemes

  ---------------------------------------------------------------------------------------
  **Template**                **Stat check**   **Target     **Successful outcome**
                                               type**       
  --------------------------- ---------------- ------------ -----------------------------
  scheme_court_donor          Charisma +       Wealthy NPC  Donor adds to your war chest.
                              Networking                    +10 personal funds, +1
                                                            endorsement track

  scheme_force_resignation    Manipulation +   Cabinet      Target resigns next reshuffle
                              Intelligence     minister or  window
                                               rival        

  scheme_win_endorsement      Negotiation +    Faction      +30 approval with that
                              Likeability      leader       faction; counts toward rank
                                                            gates

  scheme_plant_story          Manipulation +   Journalist   Narrative card slot fires
                              Press Charm      (or media    next month with chosen frame
                                               baron)       

  scheme_discredit_opponent   Intrigue +       Rival        Target loses 5--15 polling
                              Cunning          politician   points or seat marginality
  ---------------------------------------------------------------------------------------

Full 25-scheme list (P3)

Manufacture Scandal · Court Donor · Build Faction · Force Resignation ·
Discredit Opponent · Win Endorsement · Befriend · Mentor · Romance ·
Plot Coup · Found New Party · Defect · Hostile Media Takeover ·
Opposition Research · Whistleblow · Leak Document · Plant Story · Bribe
Backbencher · Deselect Rival · Sabotage Bill · Recruit SpAd · Plant Mole
in Civil Service · Coup-Proof · Buy Newspaper · Buy Broadcaster.

11.2 SPEC: scheme.progress_tick

+-----------------------------------------------------------------------+
| function TickScheme(Scheme sc, GameState s, GameRng rng):             |
|                                                                       |
| // Monthly only --- schemes don\'t progress daily                     |
|                                                                       |
| if not s.IsMonthly: return                                            |
|                                                                       |
| int progressGain = BaseProgress(sc.template_id)                       |
|                                                                       |
| \+ StatModifier(sc.owner_id, sc.template_id.stat_check)               |
|                                                                       |
| \+ AgentBonus(sc.agents)                                              |
|                                                                       |
| \+ ContextBonus(sc, s) // ally count, rank, current crises            |
|                                                                       |
| \- TargetResistance(sc.target_id, sc.template_id)                     |
|                                                                       |
| \+ rng.Next(-3, +3)                                                   |
|                                                                       |
| sc.progress = Clamp(sc.progress + progressGain, 0, 100)               |
|                                                                       |
| // Exposure roll                                                      |
|                                                                       |
| double expChance = sc.template_id.base_exposure                       |
|                                                                       |
| \- (sc.secrecy / 200.0)                                               |
|                                                                       |
| \+ (sc.target_id.IntelService.Capability / 100.0)                     |
|                                                                       |
| if rng.NextDouble() \< expChance:                                     |
|                                                                       |
| sc.state = \"exposed\"                                                |
|                                                                       |
| FireExposureConsequences(sc, s)                                       |
|                                                                       |
| return                                                                |
|                                                                       |
| // Completion                                                         |
|                                                                       |
| if sc.progress \>= 100:                                               |
|                                                                       |
| bool success = StatCheck(\...)                                        |
|                                                                       |
| sc.state = success ? \"succeeded\" : \"failed\"                       |
|                                                                       |
| FireOutcomeConsequences(sc, s)                                        |
+-----------------------------------------------------------------------+

11.3 SPEC: hooks_and_secrets

Schema in §4.4. Hooks function as Vic3-like Influence chits --- spend to
force a vote, demand a position, or coerce an action. Strong Hooks
expire on exposure; Weak Hooks last 10 in-game years.

11.4 SPEC: cabinet

  -----------------------------------------------------------------------
  **SPEC:          
  cabinet**        
  ---------------- ------------------------------------------------------
  Purpose          Suzerain-derived. Centrepiece UI showing your
                   government with mood, loyalty, and policy positions.

  Inputs           PM appointments, reshuffles, departmental events.

  Outputs          Cabinet capacity, faction reactions to composition,
                   internal resignations.

  Dependencies     Character system, faction system.

  Out of scope     Full SpAd / PPS network (P3).

  Phase            MVP (24 posts), P2 (junior ministers, \~115 posts
                   total)
  -----------------------------------------------------------------------

Cabinet loyalty mechanic (Shadow Empire-derived)

+-----------------------------------------------------------------------+
| Each minister\'s Loyalty (0..100) drifts based on:                    |
|                                                                       |
| \- Sharing player\'s ideology -\> +0.3/month                          |
|                                                                       |
| \- Being ignored (no agenda priority) -\> -0.2/month                  |
|                                                                       |
| \- Being reshuffled (sideways) -\> -10 immediate                      |
|                                                                       |
| \- Being promoted -\> +15 immediate                                   |
|                                                                       |
| \- Player honours minister\'s faction -\> +5 per honour               |
|                                                                       |
| \- Player attacks minister\'s faction -\> -10 per attack              |
|                                                                       |
| \- Held a Hook over them -\> floor of 40                              |
|                                                                       |
| If Loyalty \< 30: minister considers resigning on next adverse event  |
|                                                                       |
| If Loyalty \< 10: minister starts leaking; -10 Cabinet Secrecy        |
+-----------------------------------------------------------------------+

11.5 SPEC: cabinet.meeting

Suzerain-style monthly cabinet meeting:

-   Fires on the 1st Monday of every month.

-   3--5 named ministers present items (chosen weighted by their
    department\'s salience this month).

-   Each item has 2--4 dialogue options for the PM/leader.

-   Choices have stat checks (e.g., Negotiation 14+ to reconcile
    ministers, Tactics 16+ to spot a flaw in a proposal).

-   Outcomes feed into policy enactment, faction approval, and minister
    loyalty.

11.6 SPEC: narrative.chapter

  ----------------------------------------------------------------------------
  **SPEC:               
  narrative.chapter**   
  --------------------- ------------------------------------------------------
  Purpose               Suzerain-derived hand-authored branching dialogue
                        scene fired at key transitions.

  Inputs                Trigger condition (rank promotion, situation
                        milestone, election, etc.).

  Outputs               Branch outcome that sets new state and unlocks
                        downstream content.

  Dependencies          Character state, faction state, situation state.

  Out of scope          Voice acting. Cinematic camera.

  Phase                 P3 (8 chapters), P5 (50+ chapters)
  ----------------------------------------------------------------------------

Chapter trigger inventory (Phase 3 --- 8 chapters)

  ---------------------------------------------------------------------------
  **Trigger**                        **Working title**
  ---------------------------------- ----------------------------------------
  First election as MP (career_rank  The Count
  4→5)                               

  First Cabinet appointment (rank    The Inbox
  7→8)                               

  First party leadership bid (rank   The Hustings
  8→9)                               

  First general election as PM       The Long Campaign
  candidate (rank 9→10)              

  Coup attempt against player (any   Night of the Long Knives
  rank 8+)                           

  Major scandal threatening player   The Cliff Edge

  Constitutional crisis              Settle the Question
  (situation_trust_in_institutions ≤ 
  30)                                

  Player\'s death / retirement       The Verdict of History
  ---------------------------------------------------------------------------

12\. Game Systems --- Media & Narrative

12.1 SPEC: media.outlets

Model the UK press as \~12 named outlets at MVP, \~30 by P3. Each has:

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"outlet_telegraph\",                                         |
|                                                                       |
| \"name\": \"The Daily Telegraph\",                                    |
|                                                                       |
| \"owner_char_id\": \"char_barclay_brothers_proxy\",                   |
|                                                                       |
| \"reach_millions\": 1.6,                                              |
|                                                                       |
| \"demographic_lean\": { ABC1: +0.8, \"55+\": +0.6, conservative: +0.7 |
| },                                                                    |
|                                                                       |
| \"editorial_line_lr\": +0.6, // -1..+1 left-right                     |
|                                                                       |
| \"editorial_line_pop\": +0.2, // populist-establishment               |
|                                                                       |
| \"credibility\": 0.62,                                                |
|                                                                       |
| \"circulation_trend\": -0.04 // monthly                               |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

MVP outlet roster: Telegraph, Times, Guardian, FT, Mail, Sun, Mirror,
Express, i, BBC News, Sky News, ITV News.

12.2 SPEC: media.editorial_loop (Headliner-style)

If the player owns an outlet (corporate mogul path, or bought via
scheme), the editorial loop fires weekly:

-   3--5 incoming wire stories per week (procedurally generated from
    current world state).

-   Player chooses for each: Suppress / Run with frame A / Run with
    frame B / Sensationalise.

-   Each choice has consequences: pop ideology drift on the relevant
    axis, faction reaction, +/- credibility, +/- profit.

-   Repeated sensationalisation erodes credibility (caps at 0.15);
    excessive suppression triggers leak events.

12.3 SPEC: media.narrative_deck

The UK has a Narrative Card Deck (Civ-VI-derived): 5 slots, each filled
with a current Narrative Card. Cards modify event probabilities and pop
drift directions.

MVP card examples

  -----------------------------------------------------------------------
  **Card**        **Effect while active**
  --------------- -------------------------------------------------------
  Anti-Elite      +populist drift, -0.5 confidence in institutions/month,
  Sentiment       faction press_left & populist boosted

  Crisis Manager  PM Likeability +5; if a crisis is active, all
                  stat-checks +1

  Reformist       Strategic Initiative completion +20%; faction
  Energy          unions/civil-service +2/month

  Patriotic Mood  Monarchy +5 approval; defence spend popularity +;
                  foreign aid popularity -

  Optimism        Consumer confidence +; polling for incumbent +1/month
  -----------------------------------------------------------------------

12.4 SPEC: media.press_conference

Press conferences as a recurring stat-check mini-game (FM-derived):

-   Weekly Lobby briefing (auto-resolved by spokesperson if no player
    input).

-   Major set-piece press conference (player or spokesperson, manual): 5
    questions.

-   Each question has 4 answer postures: Direct / Deflect / Attack /
    Concede.

-   Stat checks: Composure for Direct, Manipulation for Deflect,
    Charisma+Press-Charm for Attack, Empathy+Honour for Concede.

-   Outcome modifies media\'s Narrative Spin Coefficient for the next 30
    days.

12.5 SPEC: media.journalists

\~120 named journalist NPCs (Robert Peston, Laura Kuenssberg, Beth
Rigby, Pippa Crerar, Andrew Neil, etc. --- verify before private use).
Each has FM-style attributes (Honesty, Partisanship, Sensationalism,
Reach). Player can court individual journalists via Befriend schemes;
journalists then \'soften\' coverage for player.

12.6 SPEC: media.social_media

Abstracted to 4 platforms (X, Meta, TikTok, BBC-as-aggregator). Each has
a Velocity Multiplier (how fast stories spread) and a Demographic Skew.
Player can:

-   Run paid campaigns (cost Orders + money) targeting specific
    demographic / region.

-   Have a personal account (auto-managed by SpAd; player can override
    tone weekly).

-   Be victim of viral incidents (low-roll on Press Charm during
    embarrassing event = clip goes viral).

13\. Game Systems --- Situations & Crises

13.1 SPEC: situation

  -----------------------------------------------------------------------
  **SPEC:          
  situation**      
  ---------------- ------------------------------------------------------
  Purpose          Stellaris-derived slow-burn tracked variable
                   representing a multi-decade crisis.

  Inputs           Policy choices, world events, natural drift.

  Outputs          Stage milestones that fire events; pop pressure;
                   pol-econ drag.

  Dependencies     Policy system, pop system, event system.

  Out of scope     Resolution mini-games (each situation resolves through
                   repeated policy decisions).

  Phase            MVP (1 situation: climate or fiscal crisis); P2 (10);
                   P5 (17)
  -----------------------------------------------------------------------

13.2 The 17 UK / Global Situations

  -----------------------------------------------------------------------------------
  **Situation ID**                           **Scope**             **Phase**
  ------------------------------------------ --------------------- ------------------
  situation_climate_change                   Global                MVP

  situation_ai_automation_disruption         Global                P2

  situation_demographic_ageing               UK                    P2

  situation_housing_crisis                   UK                    P2

  situation_nhs_backlog                      UK                    MVP

  situation_immigration_pressure             UK                    P2

  situation_regional_inequality              UK                    P3

  situation_brexit_aftermath                 UK                    P2

  situation_energy_transition                UK + Global           P2

  situation_productivity_stagnation          UK                    P2

  situation_debt_sustainability              UK                    MVP

  situation_sterling_crisis_risk             UK                    P3

  situation_scottish_independence_pressure   UK                    P3

  situation_ni_border_poll_pressure          UK                    P3

  situation_social_cohesion_strain           UK                    P3

  situation_political_polarisation           UK                    P3

  situation_trust_in_institutions_decline    UK                    P3
  -----------------------------------------------------------------------------------

13.3 SPEC: situation.tick

+-----------------------------------------------------------------------+
| function TickSituationMonthly(Situation sit, GameState s, GameRng     |
| rng):                                                                 |
|                                                                       |
| // Baseline drift                                                     |
|                                                                       |
| double drift = sit.monthly_drift                                      |
|                                                                       |
| // Approach modifier (each situation has 3-5 mutually exclusive       |
| approaches)                                                           |
|                                                                       |
| drift += sit.approach.drift_modifier                                  |
|                                                                       |
| // Policy contributions                                               |
|                                                                       |
| foreach contribution in sit.policy_contributions:                     |
|                                                                       |
| drift += contribution.weight \*                                       |
| (s.policies\[contribution.policy_id\].current_value -                 |
| contribution.baseline)                                                |
|                                                                       |
| // World contributions (for global situations)                        |
|                                                                       |
| if sit.scope == \"global\":                                           |
|                                                                       |
| foreach (country, weight) in sit.contributing_countries:              |
|                                                                       |
| drift += weight \*                                                    |
| s.world.countries\[country\].ContributionDrift(sit.id)                |
|                                                                       |
| sit.progress = Clamp(sit.progress + drift, 0, 100)                    |
|                                                                       |
| // Stage milestones                                                   |
|                                                                       |
| foreach milestone in sit.stage_milestones:                            |
|                                                                       |
| if not milestone.fired and sit.progress \>= milestone.at:             |
|                                                                       |
| milestone.fired = true                                                |
|                                                                       |
| FireEvent(milestone.event_template, s)                                |
|                                                                       |
| sit.stage = milestone.stage_index                                     |
+-----------------------------------------------------------------------+

13.4 SPEC: crisis (Acute events)

Acute crises sit on top of slow situations. Each is a card-deck draw
weighted by situation values:

  -----------------------------------------------------------------------
  **Crisis**          **Trigger weighting**
  ------------------- ---------------------------------------------------
  Terror attack       Base 0.5%/month, +3× if situation_social_cohesion ≥
                      60

  Financial crisis    Base 0.1%, +5× if situation_debt_sustainability ≥
  (sterling run,      80
  gilts spike)        

  Pandemic            Base 0.05%/yr; once fired, multi-year crisis arc

  Natural disaster    Base 1.0%/yr; +3× per +10 situation_climate
                      progress

  Foreign war         Driven by world state (e.g., Ukraine war scenarios)
  involvement         

  Leadership scandal  If active scheme against player is exposed; rare
                      otherwise

  By-election shock   On any MP death/resignation; outcome material if
                      marginal

  Defection           If MP loyalty \<30 and rival party offers; see
                      ITV-style 2026 Tory→Reform

  No-confidence vote  If govt majority threatened (Lab minority, etc.)

  Military mutiny     Only if situation_trust_in_institutions ≤ 20 +
                      government_type = junta

  Coup attempt        Driven by §11; only at career_rank 10
  against player      

  Mass strike         Driven by faction_unions approval \<20 + minimum
                      wage policy hostility

  Riot                Driven by social cohesion + acute event trigger
                      (e.g., police killing)

  Royal scandal       Stochastic; affects monarchy approval; player can
                      sometimes capitalise
  -----------------------------------------------------------------------

14\. Game Systems --- World & Foreign Policy

14.1 SPEC: world.countries

All 195 recognised sovereign countries modelled. Schema in §4.3. AI
tiers:

  --------------------------------------------------------------------------
  **Tier**        **Countries**              **AI complexity**
  --------------- -------------------------- -------------------------------
  Tier 1 (G20+)   \~25 (G20 + UN P5 + key    Behaviour trees with goals,
                  allies/adversaries)        priorities, response patterns

  Tier 2          \~50 (regional powers, EU  Utility AI: score actions by
  (Significant)   members, NATO members)     goal alignment

  Tier 3          \~120                      Stochastic reactions weighted
  (Default)                                  by inferred preferences
  --------------------------------------------------------------------------

14.2 SPEC: world.bilateral_relations

Each country has a per-other-country relationship: value (-100..+100),
active treaties, trade volume, sanctions. Updated by:

-   Player diplomatic actions (state visits, summits, treaties).

-   World events (war, refugee crisis, joint statement).

-   Trade flows (positive correlation).

-   Ideological alignment (autocracies drift toward each other when
    player UK is liberal-democratic, etc.).

14.3 SPEC: world.io

International organisations modelled (Stellaris Galactic
Community-derived):

  -----------------------------------------------------------------------
  **Org**                **Resolution mechanic**        **Phase**
  ---------------------- ------------------------------ -----------------
  UN (with P5 veto)      Voting strength = pop +        P4
                         economic weight; P5 veto       
                         absolute                       

  NATO (Mark Rutte       Article 5 trigger; collective  P4
  SecGen)                defence; capability targets    

  EU (UK observer        Council, Commission,           P4
  post-Brexit)           Parliament; UK joins talks if  
                         Initiative active              

  G7 / G20               Annual summits, communiqués    P4

  BRICS+                 Counter-Western coordination   P4

  WTO / IMF / World Bank Specialist regulatory bodies   P5
  / WHO                                                 
  -----------------------------------------------------------------------

14.4 SPEC: world.trade

Trade modelled at 8 sector aggregates (Goods: agri, manufacturing,
energy, raw materials; Services: finance, tech, professional, travel).
Bilateral flows computed monthly. Affected by tariffs, FTAs, sanctions,
sterling FX, ideological alignment.

14.5 SPEC: world.war

  -----------------------------------------------------------------------
  **SPEC:          
  world.war**      
  ---------------- ------------------------------------------------------
  Purpose          Shadow-Empire-OHQ-abstracted war system. NOT
                   HoI4-tactical.

  Inputs           War declaration, war goals, force commitment, named
                   generals.

  Outputs          Theatre control, casualties, public approval drag,
                   economic cost.

  Dependencies     Country system, character system (generals).

  Out of scope     Per-unit tactical map. Real-time battle simulation.

  Phase            P4
  -----------------------------------------------------------------------

War resolution per quarter:

-   Each belligerent commits N forces to T theatres.

-   Each theatre has terrain, supply, prior control.

-   OHQ command quality (named generals\' Tactics + Authority) modifies
    effective combat power.

-   Stochastic outcome: theatre control shifts by ±N%, casualties by ±M,
    depending on imbalance.

-   Peace conference if one side accepts terms; spends accumulated peace
    points on specific war goals (HoI4-derived).

14.6 SPEC: world.intelligence_ops

Terra-Invicta-derived missions on the world map. Player runs MI6 (or
whatever the player has nationalised it as) with \~12 named officers.

  ------------------------------------------------------------------------
  **Mission**   **Stat check**            **Outcome**
  ------------- ------------------------- --------------------------------
  Inspect       Intelligence officer      Reveal target country\'s hidden
  (gather info) Cunning + Concentration   stats

  Detain        Cunning + Tactics         Remove a hostile NPC from
  (rendition)                             circulation

  Turn          Manipulation + Empathy    Convert NPC to your asset
  (recruit)                               

  Assassinate   Cunning + Tactics +       Eliminate NPC (high exposure
                Resilience                risk)

  Sabotage      Tactics + Discipline      Damage building/infrastructure

  Cyber attack  Specialist cyber officer  Damage digital infrastructure,
                                          steal data, influence ops

  Disinfo       Manipulation + Press      Insert narrative card into
                Charm                     target country
  ------------------------------------------------------------------------

14.7 SPEC: world.corporates

Stellaris-MegaCorp-derived. National champions are modelled as
quasi-sovereign actors (Saudi Aramco, Samsung, Apple, Gazprom, BP,
Shell, etc.). Schema:

+-----------------------------------------------------------------------+
| {                                                                     |
|                                                                       |
| \"id\": \"corp_apple\",                                               |
|                                                                       |
| \"name\": \"Apple Inc.\",                                             |
|                                                                       |
| \"hq_country\": \"country_us\",                                       |
|                                                                       |
| \"ceo_char_id\": \"char_cook_tim\",                                   |
|                                                                       |
| \"market_cap_usd_bn\": 3200,                                          |
|                                                                       |
| \"branch_offices\": \[                                                |
|                                                                       |
| { \"country_id\": \"country_uk\", \"investment_gbp_bn\": 12.0,        |
| \"lobby_strength\": 38 },                                             |
|                                                                       |
| { \"country_id\": \"country_cn\", \"investment_gbp_bn\": 45.0,        |
| \"lobby_strength\": 22 }                                              |
|                                                                       |
| \],                                                                   |
|                                                                       |
| \"policy_preferences\": { \"policy_corp_tax_main_rate\": \"\<=        |
| 0.20\", \"policy_digital_services_tax\": \"abolish\" }                |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

15\. Game Systems --- Multi-Generational Continuity

15.1 SPEC: continuity.triple_track

On player character death/retirement, the player chooses one of three
tracks (mix and match across runs):

  -------------------------------------------------------------------------
  **Track**   **When applicable**            **Persistent entity**
  ----------- ------------------------------ ------------------------------
  Dynasty     Hereditary monarchy,           A family tree with Houses,
              family-run autocracy,          Cadet Branches, Renown
              political families             
              (Bush/Clinton/Gandhi/Bhutto)   

  Party       Democratic / one-party states  Party: Trust, donor lists,
                                             factional splits,
                                             Cadet-Branch-style splinters

  Network     Warlords, organised crime,     Network of loyalists with
              ideological movements,         relationship strengths
              corporate empires              
  -------------------------------------------------------------------------

15.2 SPEC: continuity.persistence

Across death events the following persist:

-   Party renown (5-tier legacy tree, Trust-purchased).

-   Donor lists, supporter networks.

-   Ideological inheritance: party ideology vector continues with drift.

-   Hooks held by the character (transferred to designated heir or party
    machine).

-   Secrets known by the character (some transfer, some die with them
    --- depends on whether shared).

-   Achievements: tracked toward \'Legendary\' status (Humankind Fame
    analogue).

15.3 SPEC: continuity.newgen

FM-newgen-style procedural successor generation:

+-----------------------------------------------------------------------+
| function GenerateNewgenPolitician(GameState s, NewgenContext ctx):    |
|                                                                       |
| Pop sourcePop = SamplePop(s, weights: ctx.regional_strongholds,       |
| strata: ctx.party_class_base)                                         |
|                                                                       |
| var birthYear = ctx.target_age - rng.Next(-2, +2)                     |
|                                                                       |
| var attributes = SeedAttributes(sourcePop, ctx.party.ideology)        |
|                                                                       |
| var traits = SeedTraits(sourcePop, ctx.party.ideology,                |
| ctx.dynasty_or_network_ties)                                          |
|                                                                       |
| var personalityVector = SeedPersonality(sourcePop, hooks: 0..3)       |
|                                                                       |
| return new Character(\...)                                            |
+-----------------------------------------------------------------------+

Generated newgens age into politics at 25--35. Talent Pipeline (P3) lets
player scout them earlier.

15.4 SPEC: continuity.score

Old-World-Ambitions × Humankind-Fame hybrid:

-   Each character picks 3 lifetime Ambitions at creation.

-   Completed Ambitions become permanent Legacies for the continuity
    track.

-   Each generation\'s achievements add to a Legendary Score (no cap).

-   Run-ending verdict screen shows full historical summary with score
    breakdown.

16\. UI Specifications

All screens specified below. Mockups produced during implementation;
specs here cover function only.

16.1 SPEC: ui.shell

Persistent shell elements visible during gameplay:

-   Top bar: Current date, Orders remaining, government type, prime
    minister name, polling for player party.

-   Left rail: Quick-access icons for Cabinet, Policy, Schemes,
    Situations, Map, News, Diplomacy, Treasury, Intelligence.

-   Bottom-left: Time controls (pause / 1× / 2× / 3× / 4× / 5×).

-   Bottom-right: News feed (latest 5 items, click to expand).

-   Top-right: Player character portrait, mood indicator, Energy bar,
    Stress bar.

16.2 SPEC: ui.world_map

Layer 1 --- World map. Implementation: Godot Polygon2D nodes from
TopoJSON, projected via Equal Earth or Robinson.

-   Source: topojson/world-atlas Natural Earth 1:50m (mid zoom) and
    1:110m (far zoom).

-   Per country: clickable polygon, hover tooltip (name + leader name +
    1-line ideology), click → country detail panel.

-   Country detail panel: flag, head of state portrait, ideology badge,
    GDP/population, relationship-with-UK score, treaties active,
    available diplomatic actions.

-   Layer overlays toggleable: ideology heatmap, relationship-with-UK
    heatmap, trade volume choropleth, situation contribution (e.g.,
    emissions for climate).

16.3 SPEC: ui.uk_map

Layer 2 --- UK map. Nested topology with LOD switching.

-   Source: ONS BGC Westminster Parliamentary Constituencies July 2024 +
    LAD May 2024.

-   Projection: EPSG:27700 (British National Grid).

-   LOD: zoom \<5 country boundary only; zoom 5--8 regions (NUTS-1);
    zoom 8--11 LADs; zoom \>11 constituencies.

-   Each constituency clickable → MP detail (current MP, party, 2024
    result, demographics, marginal class).

-   Overlay toggles: party colour (current incumbent), 2024 vote share,
    marginal class heat, demographic choropleth (age / income /
    ethnicity / education), polling-shift map (where seats would change
    today).

16.4 SPEC: ui.cabinet_screen

Suzerain-style centrepiece. 24 named portraits arrayed by seniority.
Each portrait shows:

-   Photo (Wikipedia thumb or pixel-portrait fallback).

-   Name, post, party.

-   Mood icon (loyal / wavering / hostile / on the brink).

-   Loyalty bar.

-   Faction lean badge.

-   Click → full character sheet panel.

Bottom bar: Reshuffle button (opens reshuffle UI), Hold Cabinet Meeting
button (queues a meeting event).

16.5 SPEC: ui.character_sheet

FM-style attribute panel. Tabs:

-   Overview: name, age, current position, ideology, key relationships.

-   Attributes: 19 visible 1--20 stats with bar rendering. Hidden
    attributes shown as \'?\' or range per Knowledge %.

-   Traits: chips listing all traits with hover-tooltip effects.

-   Career: timeline of roles held.

-   Schemes (only player\'s): active schemes targeted at this character;
    potential schemes.

-   Relationships: list of relationships with all named NPCs known to
    player.

16.6 SPEC: ui.talent_pipeline

FM-scout-network analogue. Filters by attribute, ideology, region,
faction. Shows:

-   Your party\'s current MP roster (650 sortable rows).

-   Foreign exiled politicians (Vic3 Agitators) you could invite.

-   Rising stars in rival parties you could poach (CK3 Sway).

-   Generated newgens flagged by scouts (Phase 3+).

-   Each row clickable to character sheet. Hidden attributes display
    Knowledge-% gated.

16.7 SPEC: ui.policy_web

Democracy-4-style visual node graph. Every lever is a node; connections
show synergies/conflicts; faction reaction icons on edges.
Implementation: D3-like force-directed graph in Godot, or stacked
categorical list view with cross-reference panel (faster to ship).

16.8 SPEC: ui.scheme_manager

CK3-style intrigue panel:

-   Active schemes panel: per-scheme progress bar, secrecy %, expected
    outcomes, agent slots, abort button.

-   Available schemes panel: per current rank, list of templates with
    target picker.

-   Targeted-at-me panel (P3): list of schemes other characters have
    flagged against player.

16.9 SPEC: ui.situation_log

Stellaris-style persistent panel. 5--8 ongoing slow crises with progress
bars, current approach, the player\'s monthly contribution,
time-to-next-stage estimate. Click → full situation panel with approach
selector and recent events.

16.10 SPEC: ui.strategic_initiatives

HoI4-style focus tree. Hex grid of unlockable milestones with
prerequisite arrows. Currently active initiative highlighted with
progress bar. Phase 3 unlock.

16.11 SPEC: ui.press_briefing_room

Stage view with player character (or designated spokesperson) at podium.
Reporters arrayed in audience. 5 questions presented sequentially with 4
posture options. Real-time stat-check rolls visible to player.

16.12 SPEC: ui.election_hq

Election Night view: 650-seat semicircle filling with party colours as
results come in over 30 seconds (compressed). Side panels: aggregate
seat count, swing chart by region, key marginals scoreboard.
Post-results: government formation overlay.

16.13 SPEC: ui.parliament

Semicircle of 650 seats coloured by party. Click any seat → MP detail.
Voting interface: present a bill, see vote forecast, set whip level,
hold vote, see result with named rebels.

16.14 SPEC: ui.cabinet_meeting

Suzerain dialogue layout. Centre: speaker portrait with quote bubble.
Bottom: 2--4 dialogue options (stat-check requirements shown). Right
rail: ministers present with mood indicators.

16.15 SPEC: ui.news_feed

Procedurally generated headlines (template + slots) based on world
state. Filterable by outlet. Hovering shows full article (1 paragraph).
Click → fire any associated decision events.

16.16 SPEC: ui.polling_dashboard

Sortable by demographic (age, income, region, education, ethnicity,
religion). Time-series chart of party support. Issue salience chart.
Headline: \'Polling as if election today: \[seat projection\]\'.

16.17 SPEC: ui.treasury

Full budget breakdown: revenue sources, departmental envelopes,
programme line items. OBR projection chart. Debt sustainability gauge.
Sterling FX ticker. Gilt yield curve.

16.18 SPEC: ui.diplomacy

All 195 countries listed (or sortable / filterable). Per country:
relationship score, treaties, leverages held, available diplomatic
actions, last-contact date.

16.19 SPEC: ui.intelligence

Map view of active intelligence operations. List view of named officers
with stats. Mission queue per officer. Counter-intel dashboard.

17\. Technical Architecture Details

17.1 Folder Structure

+-----------------------------------------------------------------------+
| westminster/                                                          |
|                                                                       |
| ├── /godot \# Godot project root                                      |
|                                                                       |
| │ ├── project.godot                                                   |
|                                                                       |
| │ ├── /scenes \# .tscn UI scenes                                      |
|                                                                       |
| │ ├── /scripts \# C# scripts (organised by module per §3.1)           |
|                                                                       |
| │ └── /assets                                                         |
|                                                                       |
| │ ├── /maps \# TopoJSON files                                         |
|                                                                       |
| │ ├── /portraits \# Wikipedia thumbnails + pixel art                  |
|                                                                       |
| │ ├── /flags \# 195 country flags PNG                                 |
|                                                                       |
| │ ├── /icons \# UI icons                                              |
|                                                                       |
| │ └── /audio \# Music, SFX                                            |
|                                                                       |
| ├── /content \# Game data (JSON)                                      |
|                                                                       |
| │ ├── /ideologies                                                     |
|                                                                       |
| │ ├── /policies                                                       |
|                                                                       |
| │ ├── /traits                                                         |
|                                                                       |
| │ ├── /events                                                         |
|                                                                       |
| │ ├── /characters \# Seed UK politicians                              |
|                                                                       |
| │ ├── /countries \# Seed 195 country data                             |
|                                                                       |
| │ ├── /factions                                                       |
|                                                                       |
| │ ├── /schemes \# Scheme templates                                    |
|                                                                       |
| │ ├── /situations                                                     |
|                                                                       |
| │ ├── /chapters \# Narrative chapter scripts                          |
|                                                                       |
| │ └── /initiatives \# Strategic initiative trees                      |
|                                                                       |
| ├── /data \# Raw external data (ingestion source)                     |
|                                                                       |
| │ ├── /uk \# ONS, Parliament, election data                           |
|                                                                       |
| │ └── /world \# Wikipedia ingestion, World Bank                       |
|                                                                       |
| ├── /tools \# Python / Node scripts for ingestion                     |
|                                                                       |
| │ ├── ingest_uk_mps.py                                                |
|                                                                       |
| │ ├── ingest_world_leaders.py                                         |
|                                                                       |
| │ ├── build_topojson.sh                                               |
|                                                                       |
| │ └── refresh_world_state.py                                          |
|                                                                       |
| ├── /docs \# Design docs                                              |
|                                                                       |
| │ ├── PRD.md \# this document                                         |
|                                                                       |
| │ ├── PERSONAL.md \# personal build guide                             |
|                                                                       |
| │ └── BACKLOG.md                                                      |
|                                                                       |
| └── /tests \# Unit + integration tests                                |
+-----------------------------------------------------------------------+

17.2 Save Format

Hybrid: JSON manifest + SQLite blob.

-   Filename: \`save_NN.westminster\` (renamed gzipped tar containing
    manifest.json + state.sqlite).

-   Manifest holds: game_date, RNG seed + call_count, save_version,
    game_version, player_char_id, settings.

-   SQLite holds: characters table, pops table, constituencies table
    (only mutable fields), policies table, schemes table, events table,
    world_state table.

-   Auto-save every 7 days game time. Manual save on player command.

-   Ironman mode: single rolling auto-save, no manual saves, no
    save-scumming.

17.3 Map Pipeline

+-----------------------------------------------------------------------+
| \# UK constituencies                                                  |
|                                                                       |
| ogr2ogr -f GeoJSON -t_srs EPSG:4326 raw_wpc.geojson \\                |
|                                                                       |
| WPC_BGC_2024.shp                                                      |
|                                                                       |
| mapshaper raw_wpc.geojson -simplify dp 12% keep-shapes \\             |
|                                                                       |
| -o format=topojson uk_constituencies.topojson                         |
|                                                                       |
| gzip -9 uk_constituencies.topojson \# target \< 4 MB                  |
|                                                                       |
| \# World countries                                                    |
|                                                                       |
| wget                                                                  |
| https://github.com/topojson/world-atlas/raw/master/countries-50m.json |
| \\                                                                    |
|                                                                       |
| -O world_50m.topojson                                                 |
|                                                                       |
| \# UK LADs                                                            |
|                                                                       |
| ogr2ogr -f GeoJSON -t_srs EPSG:4326 raw_lad.geojson                   |
| LAD_BGC_May2024.shp                                                   |
|                                                                       |
| mapshaper raw_lad.geojson -simplify dp 15% keep-shapes \\             |
|                                                                       |
| -o format=topojson uk_lads.topojson                                   |
+-----------------------------------------------------------------------+

17.4 SQLite Schema (Critical Tables)

+-----------------------------------------------------------------------+
| CREATE TABLE characters (                                             |
|                                                                       |
| id TEXT PRIMARY KEY,                                                  |
|                                                                       |
| name_json TEXT NOT NULL,                                              |
|                                                                       |
| birth_date TEXT, death_date TEXT,                                     |
|                                                                       |
| party_id TEXT, constituency_id TEXT, career_rank INTEGER,             |
|                                                                       |
| current_position TEXT,                                                |
|                                                                       |
| attributes_json TEXT NOT NULL,                                        |
|                                                                       |
| hidden_json TEXT NOT NULL,                                            |
|                                                                       |
| traits_json TEXT NOT NULL,                                            |
|                                                                       |
| ideology_id TEXT, ideology_purity INTEGER,                            |
|                                                                       |
| stress INTEGER, energy INTEGER, money_personal_gbp INTEGER,           |
|                                                                       |
| relationships_json TEXT, hooks_held_json TEXT, hooks_against_json     |
| TEXT,                                                                 |
|                                                                       |
| secrets_json TEXT, perks_unlocked_json TEXT, perk_xp_json TEXT,       |
|                                                                       |
| lifestyle_focus TEXT, schemes_active_json TEXT, fame INTEGER,         |
|                                                                       |
| is_player INTEGER, spawn_source TEXT                                  |
|                                                                       |
| );                                                                    |
|                                                                       |
| CREATE INDEX idx_characters_party ON characters(party_id);            |
|                                                                       |
| CREATE INDEX idx_characters_rank ON characters(career_rank);          |
|                                                                       |
| CREATE TABLE pops (                                                   |
|                                                                       |
| id INTEGER PRIMARY KEY,                                               |
|                                                                       |
| region_id TEXT, constituency_id TEXT, lad_id TEXT,                    |
|                                                                       |
| size INTEGER, stratum TEXT, profession TEXT,                          |
|                                                                       |
| ideology_json TEXT, ethnicity TEXT, religion TEXT,                    |
|                                                                       |
| age_cohort TEXT, education TEXT, engagement REAL                      |
|                                                                       |
| );                                                                    |
|                                                                       |
| CREATE INDEX idx_pops_region ON pops(region_id);                      |
|                                                                       |
| CREATE INDEX idx_pops_constituency ON pops(constituency_id);          |
|                                                                       |
| CREATE TABLE constituencies (                                         |
|                                                                       |
| id TEXT PRIMARY KEY, name TEXT, country TEXT, region TEXT,            |
|                                                                       |
| electorate INTEGER, winner_char_id TEXT,                              |
|                                                                       |
| result_2024_json TEXT, demographics_json TEXT,                        |
|                                                                       |
| topojson_object_id TEXT, lad_id TEXT                                  |
|                                                                       |
| );                                                                    |
|                                                                       |
| CREATE TABLE schemes (                                                |
|                                                                       |
| id TEXT PRIMARY KEY, template_id TEXT,                                |
|                                                                       |
| owner_id TEXT, target_id TEXT,                                        |
|                                                                       |
| progress INTEGER, secrecy INTEGER,                                    |
|                                                                       |
| started_date TEXT, expected_completion_date TEXT,                     |
|                                                                       |
| state TEXT, agents_json TEXT,                                         |
|                                                                       |
| expected_outcome TEXT, fallback_outcome TEXT                          |
|                                                                       |
| );                                                                    |
|                                                                       |
| CREATE TABLE events_fired (                                           |
|                                                                       |
| id TEXT PRIMARY KEY, fired_date TEXT,                                 |
|                                                                       |
| scope TEXT, category TEXT, headline TEXT,                             |
|                                                                       |
| consequences_applied INTEGER DEFAULT 0                                |
|                                                                       |
| );                                                                    |
+-----------------------------------------------------------------------+

17.5 RNG, Determinism, Testing

All randomness routed through GameRng (§3.5). Determinism guarantees:

-   Same seed + same player input sequence = same outcomes.

-   Save/load round-trip preserves RNG state exactly.

-   Integration test: replay a 100-month game from seed S with input log
    L; assert final state hash matches expected.

17.6 Performance Optimisations

-   Pop simulation: SQL aggregations (SUM, GROUP BY) for monthly ticks
    --- do NOT iterate pops in C#.

-   Map rendering: build polygon nodes once at startup; reuse across
    zoom levels via material switches.

-   UI updates: dirty-flag system; only re-render screens that change.

-   Event queue: priority queue sorted by fire date; process only those
    ≤ current date.

-   AI countries: tier-1 ticks weekly, tier-2 monthly, tier-3 quarterly.

-   Save: async write on background thread; show spinner if \>300ms.

17.7 Modding API (Phase 5)

All content in /content/\*.json. Mods drop into /mods/\<name\>/ with the
same folder structure; load order via /mods/load_order.txt. JSON-only
--- no script execution for safety. Override rules: file in /mods/
replaces file of same name in /content/ at load. Future Lua hooks
possible if community demand.

18\. Real-World Data Appendices

All data current to May 2026 per research. Volatility note: verify
against official sources before each seed-data refresh. Sources cited in
§23.

18.1 UK Cabinet (Starmer Ministry, post-Sept 2025 Reshuffle)

  -----------------------------------------------------------------------
  **Office**       **Holder**       **Party**    **Seat**
  ---------------- ---------------- ------------ ------------------------
  Prime Minister   Sir Keir Starmer Labour       Holborn and St Pancras

  Deputy PM &      David Lammy      Labour       Tottenham
  Justice Sec                                    

  Chancellor       Rachel Reeves    Labour       Leeds West and Pudsey

  Foreign          Yvette Cooper    Labour       Pontefract, Castleford
  Secretary                                      and Knottingley

  Home Secretary   Shabana Mahmood  Labour       Birmingham Ladywood

  Housing & LG     Steve Reed       Labour       Streatham and Croydon
                                    (Co-op)      North

  Chancellor of    Darren Jones     Labour       Bristol North West
  Duchy of                                       
  Lancaster                                      

  Chief Secretary  James Murray     Labour       Ealing North
  to the Treasury                                

  Education        Bridget          Labour       Houghton and Sunderland
  Secretary        Phillipson                    South

  Defence          John Healey      Labour       Rawmarsh and Conisbrough
  Secretary                                      

  Energy Security  Ed Miliband      Labour       Doncaster North
  & Net Zero                                     

  Environment      Emma Reynolds    Labour       (verify post-reshuffle)

  Transport        Heidi Alexander  Labour       Swindon South

  Business & Trade Jonathan         Labour       Stalybridge and Hyde
                   Reynolds                      

  Work & Pensions  Liz Kendall      Labour       Leicester West

  Culture Media &  Lisa Nandy       Labour       Wigan
  Sport                                          

  Science & Tech   Peter Kyle       Labour       Hove and Portslade

  Scotland Sec     Douglas          Labour       Lothian East
                   Alexander                     

  Wales Sec        Jo Stevens       Labour       Cardiff East

  NI Sec           Hilary Benn      Labour       Leeds South

  Leader of        Lucy Powell      Labour       Manchester Central
  Commons                                        

  Attorney General Lord Hermer KC   Labour       House of Lords
                                    (peer)       

  Chief Whip       Sir Alan         Labour       Tynemouth
                   Campbell                      

  Paymaster        Nick             Labour       Torfaen
  General          Thomas-Symonds                
  -----------------------------------------------------------------------

18.2 UK Shadow Cabinet (Badenoch, post-22 July 2025 Reshuffle)

  ------------------------------------------------------------------------
  **Office**          **Holder**                   **Seat**
  ------------------- ---------------------------- -----------------------
  Leader of the       Kemi Badenoch                North West Essex
  Opposition                                       

  Shadow Chancellor   Sir Mel Stride               Central Devon

  Shadow Foreign      Priti Patel                  Witham
  Secretary                                        

  Shadow Home         Chris Philp                  Croydon South
  Secretary                                        

  Shadow Justice      Nick Timothy (succeeded      West Suffolk
  Secretary           Jenrick Jan 2026)            

  Shadow Housing /    Sir James Cleverly           Braintree
  Deputy PM shadow                                 

  Shadow Health       Stuart Andrew                Daventry

  Shadow Defence      James Cartlidge              South Suffolk

  Shadow Education    Laura Trott                  Sevenoaks

  Shadow Energy & Net Claire Coutinho              East Surrey
  Zero                                             

  Shadow Work &       Helen Whately                Faversham and Mid Kent
  Pensions                                         

  Shadow Culture      Nigel Huddleston             Droitwich and Evesham

  Shadow Business &   Andrew Griffith              Arundel and South Downs
  Trade                                            

  Shadow Environment  Victoria Atkins              Louth and Horncastle

  Shadow Science /    Julia Lopez                  Hornchurch and
  Tech                                             Upminster

  Shadow Transport    Richard Holden               Basildon and Billericay

  Shadow Chief        Richard Fuller               North Bedfordshire
  Secretary                                        

  Shadow Cabinet      Kevin Hollinrake             Thirsk and Malton
  Office / Party                                   
  Chair                                            

  Shadow Min for      Neil O\'Brien                Harborough, Oadby and
  Policy Renewal                                   Wigston

  Chief Whip          Rebecca Harris               Castle Point
  (Opposition)                                     
  ------------------------------------------------------------------------

18.3 UK Party Leaders (May 2026)

  --------------------------------------------------------------------------
  **Party**      **Leader**       **Position**          **Notes**
  -------------- ---------------- --------------------- --------------------
  Labour         Keir Starmer     PM                    Net favourability
                                                        -45 (YouGov Apr
                                                        2026)

  Conservative   Kemi Badenoch    LOTO                  Leader since 2 Nov
                                                        2024

  Reform UK      Nigel Farage     MP, Clacton           25% polling May
                                                        2026; 8 sitting MPs

  Liberal        Sir Ed Davey     MP, Kingston &        72 seats; +60 in May
  Democrats                       Surbiton              2026 locals

  Green Party    Zack Polanski    Leader (since 2025)   200,000+ members; 4
  (E&W)                                                 MPs

  SNP            John Swinney     FM Scotland           Re-elected FM 8 May
                                                        2026; 58 MSPs

  Plaid Cymru    Rhun ap Iorwerth FM-designate Wales    Won 2026 Senedd, 43
                                                        of 96 seats

  Welsh Labour   (vacant)         ---                   Eluned Morgan
                                                        resigned 8 May 2026

  DUP            Gavin Robinson   MP, Belfast East      ---

  Sinn Féin      Mary Lou         ---                   First nationalist NI
                 McDonald (Pres)                        FM
                 / Michelle                             
                 O\'Neill (VP, NI                       
                 FM)                                    

  SDLP           Claire Hanna     MP, Belfast South &   ---
                                  Mid Down              

  Alliance       Naomi Long       MLA                   ---

  UUP            Mike Nesbitt     MLA                   ---

  TUV            Jim Allister     MP, North Antrim      ---

  Restore        Rupert Lowe      Independent MP        Founded 13 Feb 2026
  Britain                                               (ex-Reform)
  --------------------------------------------------------------------------

18.4 UK Metro Mayors, London, Devolved FMs

  --------------------------------------------------------------------------
  **Office**            **Holder**          **Party**      **Elected**
  --------------------- ------------------- -------------- -----------------
  Mayor of London       Sadiq Khan          Labour         May 2024 (3rd
                                                           term)

  Greater Manchester    Andy Burnham        Labour         May 2024

  West Midlands         Richard Parker      Labour         May 2024

  Liverpool City Region Steve Rotheram      Labour         May 2024

  West Yorkshire (incl. Tracy Brabin        Labour         May 2024
  PCC)                                                     

  South Yorkshire       Oliver Coppard      Labour         May 2024

  Tees Valley           Ben Houchen         Conservative   May 2024

  North East            Kim McGuinness      Labour         May 2024 (first
                                                           holder)

  East Midlands         Claire Ward         Labour         May 2024 (first
                                                           holder)

  York & North          David Skaith        Labour         May 2024 (first
  Yorkshire                                                holder)

  West of England       Helen Godwin        Labour         May 2025

  Cambridgeshire &      Paul Bristow        Conservative   May 2025
  Peterborough                                             

  Hampshire & Solent    (pending May 2026   ---            May 2026
                        result)                            

  First Minister,       John Swinney        SNP            Re-elected 8 May
  Scotland                                                 2026

  First Minister, Wales Rhun ap Iorwerth    Plaid Cymru    Designate May
                                                           2026

  First Minister, NI    Michelle O\'Neill   Sinn Féin      Since 3 Feb 2024

  dFM Northern Ireland  Emma                DUP            Since 3 Feb 2024
                        Little-Pengelly                    
  --------------------------------------------------------------------------

18.5 UK Institutions & Top Officials

  ------------------------------------------------------------------------
  **Office**                **Holder**                   **Since**
  ------------------------- ---------------------------- -----------------
  Monarch                   King Charles III             8 Sept 2022

  Heir Apparent             William, Prince of Wales     ---

  Cabinet Secretary / Head  Dame Antonia Romeo           Feb 2026 (first
  of Civil Service                                       woman)

  Chief of Defence Staff    ACM Sir Rich Knighton        Sept 2025

  DG MI5                    Sir Ken McCallum             Apr 2020

  Chief SIS (MI6) --- \'C\' Blaise Metreweli             2025 (first
                                                         female \'C\')

  Director GCHQ             Anne Keast-Butler            2023 (first
                                                         woman)

  Director of Public        Stephen Parkinson            Nov 2023
  Prosecutions                                           

  Lord Chief Justice        Baroness Carr of             ---
                            Walton-on-the-Hill           

  President of Supreme      Lord Reed                    ---
  Court                                                  

  Governor, Bank of England Andrew Bailey                Term to 15 March
                                                         2028

  Speaker, House of Commons Sir Lindsay Hoyle            Nov 2019

  Lord Speaker              Lord Forsyth of Drumlean     1 Feb 2026

  Archbishop of Canterbury  Dame Sarah Mullally          Installed 25
                                                         March 2026 (first
                                                         woman, 106th)

  Archbishop of York        Stephen Cottrell             ---

  CEO Ofcom                 Dame Melanie Dawes           2020

  CEO Ofgem                 Tim Jarvis (interim)         March 2026

  CEO Ofwat                 David Black                  (Ofwat under
                                                         restructuring)

  CEO FCA                   Nikhil Rathi                 Reappointed Apr
                                                         2025 to 2030

  CEO CMA                   Sarah Cardell                2022

  Information Commissioner  John Edwards                 2022
  ------------------------------------------------------------------------

18.6 World Heads of State --- Selected (May 2026)

Full 195-row list maintained in /data/world/heads_of_state.csv. Selected
key entries:

  -------------------------------------------------------------------------------
  **Country**   **Head of State**     **Head of           **Ideology**
                                      Government**        
  ------------- --------------------- ------------------- -----------------------
  United States Donald J. Trump       ---                 Right-populist
                (since 20 Jan 2025)                       (Republican)

  China         Xi Jinping (Pres.,    Li Qiang (Premier)  Marxist-Leninist
                CCP Gen Sec)                              authoritarian

  Russia        Vladimir Putin        Mikhail Mishustin   Authoritarian
                (Pres.)               (PM)                nationalist

  India         Droupadi Murmu        Narendra Modi (PM,  Hindu nationalist right
                (Pres.)               BJP)                

  Germany       Frank-Walter          Friedrich Merz      Conservative
                Steinmeier (Pres.)    (Chancellor, CDU,   
                                      since 6 May 2025)   

  France        Emmanuel Macron       Sébastien Lecornu   Liberal centrist
                (Pres.)               (PM --- verify)     

  Italy         Sergio Mattarella     Giorgia Meloni (PM, National-conservative
                (Pres.)               FdI)                right

  Japan         Emperor Naruhito      Sanae Takaichi (PM, National-conservative
                                      LDP, first female)  

  Spain         King Felipe VI        Pedro Sánchez (PM,  Centre-left
                                      PSOE)               

  Canada        King Charles III      Mark Carney (PM,    Centrist-liberal
                (rep. GG)             Liberal)            

  Australia     King Charles III      Anthony Albanese    Centre-left
                (rep. GG)             (PM, Labor)         

  Brazil        Luiz Inácio Lula da   ---                 Centre-left
                Silva (Pres., PT)                         

  Mexico        Claudia Sheinbaum     ---                 Left
                (Pres., Morena, since                     
                Oct 2024)                                 

  Argentina     Javier Milei (Pres.,  ---                 Right-libertarian
                LLA)                                      

  Chile         José Antonio Kast     ---                 Far-right
                (Pres., Republican,                       
                since 11 Mar 2026)                        

  Venezuela     DISPUTED: Delcy       ---                 Authoritarian socialist
                Rodríguez (Acting,                        / opposition
                since 3 Jan 2026) vs                      
                Edmundo González                          
                (US/EU)                                   

  Peru          José María Balcázar   ---                 Independent (8th
                (interim, since 18                        president in a decade)
                Feb 2026)                                 

  Saudi Arabia  King Salman bin       Crown Prince        Absolute monarchy
                Abdulaziz             Mohammed bin Salman 
                                      (PM)                

  UAE           Mohammed bin Zayed Al Mohammed bin Rashid Absolute monarchy /
                Nahyan (Pres.)        Al Maktoum (PM)     federal

  Iran          Mojtaba Khamenei      Masoud Pezeshkian   Theocratic
                (Supreme Leader,      (Pres., now         
                since 8/9 Mar 2026)   figurehead)         

  Israel        Isaac Herzog (Pres.)  Benjamin Netanyahu  Right
                                      (PM, Likud)         

  Syria         Ahmed al-Sharaa       (al-Sharaa heads    Sunni Islamist
                (transitional Pres.,  govt)               transitional
                since 29 Jan 2026)                        

  Egypt         Abdel Fattah el-Sisi  Mostafa Madbouly    Military-backed
                (Pres.)               (PM)                authoritarian

  Turkey        Recep Tayyip Erdoğan  ---                 Right populist
                (Pres., AKP)                              

  Nigeria       Bola Tinubu (Pres.,   ---                 Centre-right
                APC)                                      

  South Africa  Cyril Ramaphosa       ---                 Centre-left (GNU)
                (Pres., ANC)                              

  Ukraine       Volodymyr Zelenskyy   Yulia Svyrydenko    Liberal-centrist
                (Pres.)               (PM, since 17 Jul   
                                      2025)               

  Poland        Karol Nawrocki        Donald Tusk (PM,    Cohabitation: right
                (Pres., since 6 Aug   Civic Platform)     Pres + centrist PM
                2025, PiS-backed)                         

  Netherlands   King Willem-Alexander Rob Jetten (PM,     Social liberal
                                      D66, since 23 Feb   
                                      2026)               

  Sweden        King Carl XVI Gustaf  Ulf Kristersson     Centre-right
                                      (PM, Moderate)      

  Norway        King Harald V         Jonas Gahr Støre    Centre-left
                                      (PM, Labour)        

  Denmark       King Frederik X       Mette Frederiksen   Centre-left
                                      (PM, SocDem)        

  Finland       Alexander Stubb       Petteri Orpo (PM,   Centre-right
                (Pres., NCP)          NCP)                

  Ireland       Catherine Connolly    Micheál Martin      Mixed
                (Pres., ind left,     (Taoiseach, FF)     
                since 11 Nov 2025)                        

  Greece        Konstantinos Tasoulas Kyriakos Mitsotakis Centre-right
                (Pres.)               (PM, ND)            

  Portugal      António José Seguro   Luís Montenegro     Cohabitation
                (Pres., PS, since 9   (PM, PSD/AD)        
                Mar 2026)                                 

  Hungary       Tamás Sulyok (Pres.)  Viktor Orbán (PM,   National-conservative
                                      Fidesz)             

  Romania       Nicușor Dan (Pres.,   Ilie Bolojan (PM    Centre-right (govt fell
                ind, since May 2025)  caretaker, PNL)     5 May 2026)

  Czech         Petr Pavel (Pres.,    Andrej Babiš (PM,   Right-populist
  Republic      ind)                  ANO, since 9 Dec    
                                      2025)               

  Belgium       King Philippe         Bart De Wever (PM,  Flemish-nationalist
                                      N-VA)               right

  Austria       Alexander Van der     Christian Stocker   Centre-right coalition
                Bellen (Pres.)        (Chancellor, ÖVP,   
                                      since Mar 2025)     

  South Korea   Lee Jae-myung (Pres., Kim Min-seok (PM)   Centre-left
                DP, since 4 Jun 2025)                     

  Indonesia     Prabowo Subianto      ---                 Right-nationalist
                (Pres., Gerindra)                         

  Vietnam       Lương Cường (Pres.) / Phạm Minh Chính     Marxist-Leninist
                Tô Lâm (CPV Gen Sec)  (PM)                

  Thailand      King Vajiralongkorn   Anutin Charnvirakul Conservative
                                      (PM, Bhumjaithai)   

  Philippines   Ferdinand             ---                 Right
                \'Bongbong\' Marcos                       
                Jr.                                       

  Malaysia      King Sultan Ibrahim   Anwar Ibrahim (PM,  Centrist reformist
                of Johor              PKR)                

  Singapore     Tharman               Lawrence Wong (PM,  Dominant-party
                Shanmugaratnam        PAP)                technocracy
                (Pres.)                                   

  Bangladesh    Mohammed Shahabuddin  Tarique Rahman (PM, Centre-right (BNP
                (Pres.)               BNP, since \~17 Feb landslide)
                                      2026)               

  Pakistan      Asif Ali Zardari      Shehbaz Sharif (PM, Centrist
                (Pres., PPP)          PML-N)              (military-influenced)

  Taiwan        Lai Ching-te (Pres.,  Cho Jung-tai        Liberal-democratic
                DPP)                  (Premier)           

  North Korea   Kim Jong Un (WPK Gen  ---                 Totalitarian
                Sec)                                      

  New Zealand   King Charles III      Christopher Luxon   Centre-right
                (rep. GG)             (PM, National)      

  Cuba          Miguel Díaz-Canel     Manuel Marrero Cruz Communist
                (Pres. & PCC First    (PM)                
                Sec)                                      

  Iraq          Nizar Amedi (Pres.,   Ali al-Zaidi        Parliamentary mixed
                PUK, since 11 Apr     (PM-designate)      
                2026)                                     
  -------------------------------------------------------------------------------

18.7 IO Heads (May 2026)

  -----------------------------------------------------------------------
  **Organisation**               **Head**
  ------------------------------ ----------------------------------------
  UN Secretary-General           António Guterres

  NATO Secretary General         Mark Rutte (since 1 Oct 2024)

  European Commission President  Ursula von der Leyen (re-elected July
                                 2024, to 2029)

  European Council President     António Costa

  European Parliament President  Roberta Metsola

  EU High Rep for Foreign        Kaja Kallas
  Affairs                        

  ECB President                  Christine Lagarde (term to 2027)

  IMF Managing Director          Kristalina Georgieva

  World Bank President           Ajay Banga

  WHO Director-General           Tedros Adhanom Ghebreyesus

  WTO Director-General           Ngozi Okonjo-Iweala

  AU Commission Chair            Mahmoud Ali Youssouf

  OPEC Secretary-General         Haitham Al Ghais
  -----------------------------------------------------------------------

18.8 Active Major Crises (May 2026)

-   Ukraine war --- 5th year. Russia controls \~18% of Ukraine;
    ceasefires (Easter, May 5-6, May 8-9) failed. US-brokered talks
    ongoing. Yulia Svyrydenko PM of Ukraine since Jul 2025.

-   2026 Iran war (28 Feb -- 5 May 2026) --- US/Israel \'Operation Epic
    Fury\'. Killed Ali Khamenei 28 Feb 2026. Mojtaba Khamenei succeeded
    8/9 March. Strait of Hormuz partially closed. Ceasefire 8 Apr →
    formal end 5 May.

-   2026 Lebanon war (since 2 Mar 2026) --- resumed Hezbollah-Israel
    conflict. \>2,800 dead; \>1 million displaced. 10-day truce extended
    45 days 15 May.

-   Gaza --- Trump-Netanyahu Sept 2025 peace plan stuck in phase one.
    \~50% Israeli occupation; \~400 killed since \'ceasefire\'.

-   Taiwan --- Lai presidency under PRC military pressure; no kinetic
    conflict.

-   Sudan civil war (SAF vs RSF) --- continues; world\'s largest
    displacement crisis.

-   Myanmar --- junta losing ground.

-   Venezuela --- US claimed capture of Maduro 3 Jan 2026; disputed
    succession.

-   UK domestic --- cost-of-living, NHS waiting lists, Channel
    crossings, Reform surge polling 25%, May 2026 locals (Ref +1,451
    seats net).

-   2026 elections to watch --- Bangladesh (held Feb), Netherlands (held
    Oct 2025 → Jetten), Chile (Dec 2025 → Kast), Iraq, Hungary 2026.

18.9 Data Source URLs

  ----------------------------------------------------------------------------------------------------------------------------------------------
  **Dataset**           **URL**
  --------------------- ------------------------------------------------------------------------------------------------------------------------
  UK Cabinet            members.parliament.uk/government/cabinet

  UK Shadow Cabinet     members.parliament.uk/opposition/cabinet

  2024 GE results       commonslibrary.parliament.uk/research-briefings/cbp-10009

  ONS Constituency      geoportal.statistics.gov.uk/datasets/ons::westminster-parliamentary-constituencies-july-2024-boundaries-uk-bgc-2/about
  boundaries            

  ONS LAD boundaries    geoportal.statistics.gov.uk/ (search \'LAD May 2024 BGC\')

  Natural Earth         github.com/topojson/world-atlas
  TopoJSON              

  Heads of state        en.wikipedia.org/wiki/List_of_current_heads_of_state_and_government

  NATO Sec Gen          nato.int/en/about-us/organization/nato-structure/nato-secretary-general

  BoE Governor          bankofengland.co.uk/about/people/andrew-bailey/biography

  EU Commission         commission.europa.eu/about/organisation/president_en

  UK Parliament Members members-api.parliament.uk/
  API                   

  YouGov polling        yougov.co.uk/topics/politics

  World Bank Open Data  data.worldbank.org/
  ----------------------------------------------------------------------------------------------------------------------------------------------

19\. Development Roadmap

19.1 Phase 0 --- Paper & Data (1 month)

-   Lock all schemas from §4. Validate JSON files load cleanly.

-   Ingest seed data: 24 Cabinet, 20 Shadow Cabinet, 15 party leaders,
    17 metro mayors/FMs, 21 institutions, 56 world heads of state.

-   Run map pipeline: produce uk_constituencies.topojson (BGC),
    uk_lads.topojson (BGC), world_50m.topojson.

-   Author MVP content: 50 policies, 20 traits, 10 ideologies, 5 scheme
    templates, 1 situation.

-   Definition of done: CI loads all content files without error; data
    is committed to git.

19.2 Phase 1 --- MVP / Text-only prototype (5 months)

-   Implement schemas as C# records.

-   Implement Core (Rng, time, GameState).

-   Implement character creation + save/load.

-   Implement policy engine with 50 levers.

-   Implement 1,000-pop simulation across 12 regions.

-   Implement 1 election cycle (FPTP, 650 seats).

-   Implement 5 scheme templates.

-   Implement Cabinet with 24 named seats (seeded May 2026 data).

-   Implement UK map (basic, BGC topology, clickable constituencies).

-   Implement world map (basic, country flags + leader photos).

-   Definition of done: player can rise from MP → Cabinet → PM →
    win/lose election in ≤ 4 hours.

19.3 Phase 2 --- Depth (4 months)

-   Expand policy catalogue to \~200 levers (P2-tagged).

-   Implement full faction system (13 factions) with approval,
    petitions, reactions.

-   Implement press conferences mini-game.

-   Implement Narrative Card Deck v1 (15 cards).

-   Expand pops to 6,000 mapped to 650 constituencies.

-   Implement Vic3-style three-phase law enactment.

-   Add 10 situations.

-   Implement local elections (annual May).

-   Implement multi-generational Party Track succession.

-   Definition of done: 30-year campaign across 3 leaders feels
    coherent; stories naturally emerge.

19.4 Phase 3 --- Intrigue & Narrative (4 months)

-   Full scheme system: 25 templates with hidden attributes / Knowledge
    revealed via interaction.

-   Hand-author 8 narrative chapters covering rank gates and crises.

-   Full cabinet management (loyalty, reshuffle, resignation chains).

-   Constitutional reform Strategic Initiative path.

-   Talent Pipeline UI for scouting political talent.

-   Stress & Coping system.

-   Lifestyle perk trees (5 trees × 25 perks).

-   Add remaining situations.

-   Definition of done: single playthrough rated by external playtester
    as \'I want to play again\'.

19.5 Phase 4 --- World Layer (5 months)

-   195 country AIs (tiered as per §14.1).

-   Major IOs (UN, NATO, EU, G7/G20).

-   Shadow-Empire-OHQ war system.

-   Intelligence operations (Terra Invicta-style missions on world map).

-   Sanctions menus.

-   Corporate moguls (Stellaris MegaCorp branch offices).

-   Foreign policy Strategic Initiative trees.

-   Climate change end-game crisis.

-   Definition of done: player can lead a coalition into a UN-mandated
    intervention and see global consequences.

19.6 Phase 5 --- Multi-Generational & Modding (3+ months)

-   Dynasty / Party / Network triple-track succession fully implemented.

-   FM-style newgen system seeded from pops.

-   Full 525 policy lever catalogue.

-   JSON modding API with /mods/ folder + load order.

-   50+ narrative chapters covering all major life events.

-   All 30+ ideologies + custom builder.

-   All 15 government types + transition mechanics.

-   Definition of done: player completes a 2-character, 30-year campaign
    with all major systems engaged.

+-----------------------------------------------------------------------+
| **Total estimate**                                                    |
|                                                                       |
| \~22 months solo (evenings + weekends, \~10 hrs/week) to v1.0. Phase  |
| 1 is the only deadline that matters.                                  |
+-----------------------------------------------------------------------+

20\. Test Cases & Acceptance Criteria

Each system has acceptance criteria already in its SPEC block. Below are
integration test cases that exercise multiple systems.

20.1 Integration Test: Backbench → PM (MVP)

+-----------------------------------------------------------------------+
| TEST: integration.career.backbench_to_pm                              |
|                                                                       |
| Setup:                                                                |
|                                                                       |
| \- Seed game at 1 May 2026 with May 2026 data                         |
|                                                                       |
| \- Player character: Labour backbencher in a marginal seat            |
|                                                                       |
| \- Attributes: balanced (all 12)                                      |
|                                                                       |
| Steps:                                                                |
|                                                                       |
| 1\. Advance 6 months. Verify player can be appointed PPS by           |
| stat-check or scheme.                                                 |
|                                                                       |
| 2\. Force reshuffle event. Verify player promoted to Jr Minister.     |
|                                                                       |
| 3\. Advance 24 months. Manufacture Scandal scheme on Cabinet rival.   |
|                                                                       |
| 4\. Verify rival resigns; player promoted to Cabinet.                 |
|                                                                       |
| 5\. Trigger Topple Leader scheme on Starmer (after 36 months).        |
|                                                                       |
| 6\. Win leadership contest with 55%+ MP and 60%+ members vote.        |
|                                                                       |
| 7\. Verify player is Leader of Opposition or PM (depending on         |
| government state).                                                    |
|                                                                       |
| Pass: career_rank advanced from 5 → 10 within 48 months.              |
+-----------------------------------------------------------------------+

20.2 Integration Test: Policy → Pops → Election

+-----------------------------------------------------------------------+
| TEST: integration.policy_pops_election                                |
|                                                                       |
| Setup:                                                                |
|                                                                       |
| \- Seed game at 1 May 2026, player is PM with Labour majority         |
|                                                                       |
| Steps:                                                                |
|                                                                       |
| 1\. Set policy_income_tax_higher_rate to 0.55 (high) at month 1.      |
|                                                                       |
| 2\. Advance 12 months. Query pop ideology shift on economic_lr axis.  |
|                                                                       |
| Expect: wealthy stratum drifts +0.04 to +0.08 economic_lr (rightward) |
|                                                                       |
| Expect: middle stratum drifts -0.02 to +0.02 (mixed)                  |
|                                                                       |
| 3\. Advance to next election. Run ResolveFptpElection.                |
|                                                                       |
| Expect: Conservative + Reform seat share rises ≥ 5 points vs baseline |
|                                                                       |
| Expect: Labour majority shrinks or is lost                            |
|                                                                       |
| 4\. Verify faction_city_finance approval ≤ baseline - 15              |
|                                                                       |
| Pass: All three numeric expectations met within ±2pp.                 |
+-----------------------------------------------------------------------+

20.3 Integration Test: Constitutional Reform

+-----------------------------------------------------------------------+
| TEST: integration.constitutional.lords_abolition                      |
|                                                                       |
| Setup:                                                                |
|                                                                       |
| \- Player is PM with comfortable majority and progressive ideology    |
|                                                                       |
| Steps:                                                                |
|                                                                       |
| 1\. Trigger initiative_uk_constitution_abolish_lords.                 |
|                                                                       |
| 2\. Verify Order cost per year applied.                               |
|                                                                       |
| 3\. Verify three-phase enactment fires (Drafting → Debate → Royal     |
| Assent).                                                              |
|                                                                       |
| 4\. Lords resist via ping-pong. Verify Parliament Acts can override   |
| after 1 year delay.                                                   |
|                                                                       |
| 5\. King considers withholding Royal Assent. Verify                   |
| situation_trust_in_institutions delta if withheld.                    |
|                                                                       |
| 6\. After full passage: uk.lords replaced with unicameral fallback    |
| configuration.                                                        |
|                                                                       |
| Pass: Lords absent from game state; situation_political_polarisation  |
| +5; faction_monarchy approval -25; legitimacy floor for               |
| parliamentary_constitutional_monarchy maintained or shifted to        |
| parliamentary_republic.                                               |
+-----------------------------------------------------------------------+

20.4 Integration Test: Save/Load Determinism

+-----------------------------------------------------------------------+
| TEST: integration.persistence.determinism                             |
|                                                                       |
| Setup:                                                                |
|                                                                       |
| \- Seed S1 = 8472918374                                               |
|                                                                       |
| Steps:                                                                |
|                                                                       |
| 1\. Start a game with seed S1. Record input log L for 100             |
| month-ticks.                                                          |
|                                                                       |
| 2\. At month 100, hash final GameState (excluding UI fields). Record  |
| H1.                                                                   |
|                                                                       |
| 3\. Save game to disk. Quit.                                          |
|                                                                       |
| 4\. Reload save. Verify game date and player position match.          |
|                                                                       |
| 5\. Replay input log L from a fresh start with seed S1.               |
|                                                                       |
| 6\. Hash final GameState at month 100. Record H2.                     |
|                                                                       |
| Pass: H1 == H2 (byte-exact). Save/load round-trip preserves state.    |
+-----------------------------------------------------------------------+

20.5 Performance Acceptance Tests

  ------------------------------------------------------------------------
  **Test**                     **Target**            **Hard fail
                                                     threshold**
  ---------------------------- --------------------- ---------------------
  Monthly tick (full pop +     \< 40 ms              \> 100 ms
  policy + situation)                                

  Election Night (650 seats    \< 30 seconds wall    \> 60 seconds
  resolved)                    time                  

  UK map render at             60 fps sustained      \< 30 fps
  constituency zoom                                  

  Save full game state         \< 500 ms             \> 2 s

  Cold load                    \< 2 seconds          \> 5 seconds

  Memory at 60 game years      \< 1.5 GB             \> 4 GB
  ------------------------------------------------------------------------

21\. Risks & Mitigations

  ----------------------------------------------------------------------------
  **Risk**                **Severity**   **Mitigation**
  ----------------------- -------------- -------------------------------------
  Scope creep (\'infinite Critical       Lock MVP at 50 policies. New ideas go
  policy\' ambition)                     to BACKLOG.md, not code. Phase 2 only
                                         begins after MVP ships.

  Simulation balancing    High           Build automated regression sim: 1,000
  nightmare                              AI-only games per nightly build,
                                         expect macro indicators within
                                         bounds.

  Power-&-Revolution      High           Vertical-slice every system before
  problem (breadth                       adding the next domain. Each phase
  without depth)                         must deliver a playable experience,
                                         not a feature checklist.

  AI for 195 countries    Medium         Tiered AI as per §14.1. Tier 3 is
                                         intentionally shallow.

  Performance at scale    Medium         WebGL/Godot Polygon2D batching. SQL
  (650 polygons + 195                    aggregation for pops. Profile early,
  countries + 6,000 pops)                profile often.

  Content cost of         High           Chapter editor as a Phase 2
  narrative chapters                     deliverable. Author 8 in P3;
                                         community-mod the rest in P5.

  Real-name legal         Medium         Document is for personal hobby use
  exposure                (mitigated by  only. Do not publish or commercialise
                          private use)   without legal review. Build a
                                         \'thinly-veiled\' mode for any public
                                         release.

  Political data churn    High           Build refresh_world_state.py to
                                         re-ingest Wikipedia + Parliament data
                                         every 3 months. Version world state
                                         by date.

  Burnout (solo dev)      High           Public playable build every 3 months
                                         (privately, to one trusted friend).
                                         Private demo to friends monthly.

  Suzerain narrative      Medium         Use parameterised chapter templates
  quality at scale                       with stat-checks; 80% procedural, 20%
                                         hand-authored set pieces.
  ----------------------------------------------------------------------------

22\. Open Design Questions

Decisions deferred until implementation. AI assistants: if a section
above conflicts with an answer here, the answer here is authoritative.

  ----------------------------------------------------------------------------
  **\#**   **Question**                    **Recommended default**
  -------- ------------------------------- -----------------------------------
  Q1       Real-time-with-pause vs         RT-with-pause. 2 sec/day at speed
           turn-based?                     3. Auto-pause on major events.

  Q2       Save policy?                    Auto-save every 7 game days. Manual
                                           save allowed except in Ironman
                                           mode.

  Q3       Multiplayer scope?              Out of scope for v1. Consider async
                                           play-by-mail only as a post-v1
                                           stretch.

  Q4       AI difficulty tiers?            Five tiers. AI handicaps via
                                           economic multipliers, not
                                           omniscience.

  Q5       Campaign length cap?            Soft cap 60 years; no hard end.
                                           Permanent dynasty/party/network
                                           mode.

  Q6       Death of starting character?    Choice point: continue dynasty /
                                           continue party / continue network /
                                           retire run.

  Q7       Modding scope at v1?            JSON-only, no script execution, for
                                           safety. Lua hooks possible in v2.

  Q8       Player portrait?                Pixel art (Aseprite, \~32x32 or
                                           48x48). Generated from feature
                                           options at character creation.

  Q9       How to handle real              World refresh tool re-ingests;
           politicians\' deaths after seed existing saves retain seed state.
           date?                           

  Q10      Censorship / political          None for private use. Player can
           sensitivity?                    implement fascist policies;
                                           simulation responds realistically.
  ----------------------------------------------------------------------------

23\. Source Citations

Primary sources for real-world data in §18, ordered alphabetically.
Re-verify before each seed-data refresh.

12. AJ Bell. \'Starmer makes major Cabinet reshuffle after Rayner
    quits\' (Sept 2025).

13. Al Jazeera. \'Sinn Féin\'s Michelle O\'Neill appointed Northern
    Ireland\'s first minister\' (Feb 2024).

14. Al Jazeera. \'Iran names Mojtaba Khamenei as new supreme leader\' (8
    Mar 2026).

15. Al Jazeera. \'Bangladesh\'s interim leader Yunus steps down\' (16
    Feb 2026).

16. Al Jazeera. \'Iraqi president names Shia bloc candidate Ali al-Zaidi
    as PM-designate\' (27 Apr 2026).

17. Bloomberg. \'Badenoch Shuffles UK Tory Team in Bid to Salvage
    Leadership\' (22 Jul 2025).

18. Brevia Consulting. \'Who is the new Conservative Party Leader and
    their Shadow Cabinet?\'

19. Britannica. \'2026 Iran war\' (Encyclopaedia entry).

20. Daily Post. \'Plaid Cymru declares victory with Rhun ap Iorwerth set
    to be new First Minister\' (May 2026).

21. Euronews. \'Right-wing billionaire Andrej Babiš sworn in as Czech
    PM\' (9 Dec 2025).

22. Globalleadersinsights. \'Top 10 Leaders In Geo-Politics Realm ---
    2026\'.

23. House of Commons Library. \'A new president and prime minister for
    Iraq\' (CBP-10829).

24. House of Commons Library. \'General Election 2024 results\'
    (CBP-10009).

25. Institute for Government. \'Scottish elections 2026: Scottish
    parliament\'.

26. Institute for Government. \'Welsh elections 2026: Senedd Cymru\'.

27. ITV News. \'All the former Tories who have defected to Reform UK\'
    (15 Jan 2026).

28. LabourList. \'Antonia Romeo appointed to lead civil service as new
    Cabinet Secretary\' (Feb 2026).

29. MI5. \'Director General\' (official biography).

30. MoneyCheck. \'UK FCA Reappoints Nikhil Rathi as CEO Through 2030\'.

31. NATO. \'NATO Secretary General\' (official page).

32. NBC News. \'Britain\'s MI6 spy agency gets its first female chief\'.

33. NBC News. \'Iranian Supreme Leader Mojtaba Khamenei vows vengeance
    in fiery first statement\'.

34. Nation.Cymru. \'Senedd Election 2026 Live\'.

35. NPR. \'Chile shifts sharply right as José Antonio Kast wins the
    presidency\' (14 Dec 2025).

36. NPR. \'Iran names Mojtaba Khamenei as its new supreme leader\' (8
    Mar 2026).

37. ONS Open Geography Portal. \'Westminster Parliamentary
    Constituencies (July 2024) Boundaries UK BGC\'.

38. OPB. \'José María Balcázar becomes Peru\'s eighth president in a
    decade\' (19 Feb 2026).

39. PBS. \'Russia and Ukraine trade blame for continued fighting\' (May
    2026).

40. PkgPulse. \'Mapbox vs Leaflet vs MapLibre: Maps 2026\' (technical
    comparison).

41. ProCapitas. \'Wales Senedd Election 2026 Results: Plaid Cymru
    Wins\'.

42. SIS (MI6). \'Appointment of the new Chief of the Secret Intelligence
    Service\' (official).

43. Stimson Center. \'More Spasms of Violence Await the Middle East in
    2026\'.

44. TIME. \'Trump Ally Nigel Farage Gains in U.K. Local Elections as
    Starmer\'s Future Uncertain\' (8 May 2026).

45. Topojson world-atlas. \'Natural Earth derivative TopoJSON\'
    (github.com/topojson/world-atlas).

46. UK Parliament. \'His Majesty\'s Government: The Cabinet\'
    (members.parliament.uk/government/cabinet).

47. UK Parliament. \'His Majesty\'s Official Opposition: The Shadow
    Cabinet\' (members.parliament.uk/opposition/cabinet).

48. UK Parliament. \'Lord Forsyth of Drumlean is next Lord Speaker\'.

49. UK Parliament. \'Lord Speaker to step down in February 2026\'.

50. Wikipedia. \'List of current heads of state and government\'
    (en.wikipedia.org).

51. Wikipedia. \'Starmer ministry\' (en.wikipedia.org).

52. Wikipedia. \'Badenoch shadow cabinet\' (en.wikipedia.org).

53. Wikipedia. \'Reform UK\' (en.wikipedia.org).

54. Wikipedia. \'2026 in United Kingdom politics and government\'
    (en.wikipedia.org).

55. Wikipedia. \'2026 Scottish Parliament election\' (en.wikipedia.org).

56. Wikipedia. \'2026 Senedd election\' (en.wikipedia.org).

57. Wikipedia. \'2026 British cabinet reshuffle\' (en.wikipedia.org).

58. Wikipedia. \'2026 Iran war\' (en.wikipedia.org).

59. Wikipedia. \'2026 Lebanon war\' (en.wikipedia.org).

60. Wikipedia. \'2026 Iranian supreme leader election\'
    (en.wikipedia.org).

61. Wikipedia. \'Friedrich Merz\' (en.wikipedia.org).

62. Wikipedia. \'Mark Rutte\' (en.wikipedia.org).

63. Wikipedia. \'Kemi Badenoch\' (en.wikipedia.org).

64. Wikipedia. \'Sarah Mullally\' (en.wikipedia.org).

65. Wikipedia. \'Eluned Morgan\' (en.wikipedia.org).

66. Wikipedia. \'Ilie Bolojan\' (en.wikipedia.org).

67. Wikipedia. \'Nicușor Dan\' (en.wikipedia.org).

68. Wikipedia. \'Anutin Charnvirakul\' (en.wikipedia.org).

69. Wikipedia. \'Michelle O\'Neill\' (en.wikipedia.org).

70. Wikipedia. \'Governor of the Bank of England\' (Andrew Bailey).

71. Wikipedia. \'Chief of the Defence Staff (United Kingdom)\'.

72. Wikipedia. \'Schoof cabinet\' (Netherlands transition).

73. Wikipedia. \'2025 Norwegian parliamentary election\'.

74. Wikipedia. \'Directly elected mayors in England\'.

75. Wikipedia. \'G20\' (current chair).

76. World Population Review. \'Titles of Leaders of Countries 2026\'.

77. Worldcrunch. \'This Video Game Grooms Future World Leaders\'
    (Eversim interview).

78. YouGov. \'Political favourability ratings, April 2026\'.

79. YouGov. \'Voting intention, 4-5 May 2026: Ref 25%, Lab 18%, Con 17%,
    Grn 15%, LD 14%\'.

**--- END OF PRD ---**

Document length: \~50 pages · v1.0 · May 2026

*Now build the thing.*
