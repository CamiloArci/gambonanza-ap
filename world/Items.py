from typing import Dict, NamedTuple, Optional
from BaseClasses import ItemClassification

class ItemData(NamedTuple):
    code: int
    classification: ItemClassification

BASE_ID = 60000

item_table: Dict[str, ItemData] = {
    # Pieces
    "Pawn": ItemData(BASE_ID + 1, ItemClassification.useful),
    "Rook": ItemData(BASE_ID + 2, ItemClassification.useful),
    "Knight": ItemData(BASE_ID + 3, ItemClassification.useful),
    "Bishop": ItemData(BASE_ID + 4, ItemClassification.useful),
    "King": ItemData(BASE_ID + 5, ItemClassification.useful),
    "Queen": ItemData(BASE_ID + 6, ItemClassification.useful),

    # Tiles
    "Golden Tile": ItemData(BASE_ID + 7, ItemClassification.useful),
    "Protective Tile": ItemData(BASE_ID + 8, ItemClassification.useful),
    "Blessing Tile": ItemData(BASE_ID + 9, ItemClassification.useful),
    "Trap Tile": ItemData(BASE_ID + 10, ItemClassification.useful),
    "Phantom Tile": ItemData(BASE_ID + 11, ItemClassification.useful),
    "Cursed Tile": ItemData(BASE_ID + 12, ItemClassification.trap),

    # Gambits - Common
    "Bug Catcher's Gambit": ItemData(BASE_ID + 13, ItemClassification.useful),
    "Squirrel's Gambit": ItemData(BASE_ID + 14, ItemClassification.useful),
    "Proliferation's Gambit": ItemData(BASE_ID + 15, ItemClassification.useful),
    "Race Flag's Gambit": ItemData(BASE_ID + 16, ItemClassification.useful),
    "Caterpillar's Gambit": ItemData(BASE_ID + 17, ItemClassification.useful),
    "Dungeon's Gambit": ItemData(BASE_ID + 18, ItemClassification.useful),

    # Gambits - Rare
    "Hidden Queen's Gambit": ItemData(BASE_ID + 19, ItemClassification.useful),
    "Fairy's Gambit": ItemData(BASE_ID + 20, ItemClassification.useful),
    "Winter Helmet's Gambit": ItemData(BASE_ID + 21, ItemClassification.useful),

    # Gambits - Epic
    "Spy's Gambit": ItemData(BASE_ID + 22, ItemClassification.useful),
    "Lemmong's Gambit": ItemData(BASE_ID + 23, ItemClassification.useful),

    # Gambits - Legendary
    "Thunder's Gambit": ItemData(BASE_ID + 24, ItemClassification.useful),

    # Board Upgrades
    "Board Upgrade": ItemData(BASE_ID + 25, ItemClassification.useful),
}
