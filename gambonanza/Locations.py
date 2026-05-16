from typing import Dict, NamedTuple, Optional

class LocationData(NamedTuple):
    code: int

BASE_ID = 60000

# 31 Shop Checks
location_table: Dict[str, LocationData] = {
    f"Shop Check {i}": LocationData(BASE_ID + i)
    for i in range(1, 32)
}
