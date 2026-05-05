from typing import Dict, NamedTuple, Optional

class LocationData(NamedTuple):
    code: int

BASE_ID = 60000

# Total 31 Locations
location_table: Dict[str, LocationData] = {
    # 25 Matches (5 Stages, 5 Games each)
    **{f"Match {stage}-{game}": LocationData(BASE_ID + (stage-1)*5 + game)
       for stage in range(1, 6)
       for game in range(1, 6)},
    
    # 5 Bosses (End of each stage)
    **{f"Boss {stage}": LocationData(BASE_ID + 25 + stage)
       for stage in range(1, 6)},
    
    # 1 Final Goal
    "Game Clear": LocationData(BASE_ID + 31),
}
