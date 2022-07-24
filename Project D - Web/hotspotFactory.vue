<template>
  <div>
    <div v-for="hotspot in hotspots" :key="hotspot.id">
      <Hotspot
        :show="show"
        :id="hotspot.id"
        :imageUrl="imageUrl ? imageUrl : hotspot.imageUrl"
        :positionX="
          positions && positions.length > hotspot.id && positions[hotspot.id]
            ? positions[hotspot.id].positionX
            : -500
        "
        :positionY="
          positions && positions.length > hotspot.id && positions[hotspot.id]
            ? positions[hotspot.id].positionY
            : -500
        "
        :offsetX="hotspot.offsetX"
        :offsetY="hotspot.offsetY"
        :sizeX="hotspot.sizeX"
        :sizeY="hotspot.sizeY"
        :labelText="hotspot.labelText"
        :textAlignX="hotspot.textAlignX"
        :textAlignY="hotspot.textAlignY"
        :textOffsetX="hotspot.textOffsetX"
        :textOffsetY="hotspot.textOffsetY"
        :zIndex="
          positions && positions.length > hotspot.id && positions[hotspot.id]
            ? positions[hotspot.id].zIndex
            : -500
        "
        :isButton="hotspot.isButton"
      ></Hotspot>
    </div>
  </div>
</template>

<script>
import Hotspot from "./hotspot";
export default {
  name: "HotspotFactory",
  components: { Hotspot },
  data() {
    return {
      show: false,
      hotspots: null,
      imageUrl: null,
      positions: null,
    };
  },
  props: {},
  mounted() {
    pc.app.on("hotspots:show", (data) => {
      // this.show = false;
      this.hotspots = data.hotspots ? data.hotspots : null;
      // console.log("show");
      this.imageUrl = data.imageUrl ? data.imageUrl : null;
      setTimeout(() => {
        this.show = true;
      }, 1);
    });
    pc.app.on("hotspots:update", (data) => {
      this.positions = data;
    });
    pc.app.on("hotspots:hide", (data) => {
      this.show = false;
    });
    pc.app.on("hotspots:clear", (data) => {
      this.show = false;
      this.hotspots = null;
      this.positions = null;
    });
  },
  computed: {},
};
</script>