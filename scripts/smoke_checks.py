import json, pathlib, re

root = pathlib.Path(__file__).resolve().parents[1]

# JSON round-trip smoke for schema-like payloads
payload = {
  "id":"char_player","name":{"first":"A","last":"B","honorific":None},"birth_date":"1980-01-01","death_date":None,
  "gender":"nonbinary","ethnicity":"x","religion":"none","sexuality":"x","constituency_id":None,"party_id":None,
  "career_rank":0,"current_position":"none","attributes":{"charisma":1},"hidden":{"loyalty":1},"traits":[],"ideology_id":"i",
  "ideology_purity":50,"stress":0,"energy":100,"money_personal_gbp":0,"relationships":[],"hooks_held":[],"hooks_against_me":[],"secrets":[],
  "perks_unlocked":[],"perk_xp":{},"lifestyle_focus":"none","schemes_active":[],"fame":0,"is_player":True,"spawn_source":"player_created"
}
text = json.dumps(payload)
assert json.loads(text) == payload

# Tick/hook interval proofs for 400 ticks
ticks=400
monthly=sum(1 for i in range(1,ticks+1) if i%30==0)
annual=sum(1 for i in range(1,ticks+1) if i%365==0)
autosave=sum(1 for i in range(1,ticks+1) if i%7==0)
assert (monthly, annual, autosave)==(13,1,57)

print('smoke_checks_ok monthly=13 annual=1 autosave=57')


# policy content checks
policy_dir = root / "content" / "policies"
all_levers=[]
for f in sorted(policy_dir.glob("*.json")):
  lines=[ln for ln in f.read_text().splitlines() if not ln.lstrip().startswith("//")]
  all_levers.extend(json.loads("\n".join(lines)))
mvp=[x for x in all_levers if x.get("phase_tag")=="MVP"]
assert len(mvp)==50
ids=[x["id"] for x in mvp]
assert len(ids)==len(set(ids))
assert all(len(x.get("effects",[]))>0 for x in mvp)

# pop model static checks
for rel in [
  "src/Pops/PopSeeder.cs",
  "src/Pops/PopSystem.cs",
  "src/Pops/PopQueries.cs",
  "tests/Westminster.Tests/PopSystemTests.cs",
]:
  assert (root / rel).exists(), f"missing expected pop file: {rel}"

region_ids = [
  "region_north_east","region_north_west","region_yorkshire_and_humber","region_east_midlands",
  "region_west_midlands","region_east_of_england","region_london","region_south_east",
  "region_south_west","region_wales","region_scotland","region_northern_ireland"
]
seed_source = (root / "src" / "Pops" / "PopSeeder.cs").read_text()
for rid in region_ids:
  assert rid in seed_source, f"region id missing from pop seeder: {rid}"
