<template>
  <div :style="hsPosition">
    <transition name="blend">
      <button
        v-if="show"
        :class="`pc-button ${this.isButton ? 'hotspot' : ''}`"
        :style="btnStyle"
        @click="onSelect"
        @touchstart="onTouchStart"
        :disabled="!isButton"
      >
        <div :style="hsStyle">
          <img
            onmousedown="return false"
            draggable="false"
            v-if="imageUrl"
            :src="imageUrl"
            :id="id"
          />
          <div v-if="labelText" :style="textStyle" class="txt">
            {{ labelText }}
          </div>
        </div>
      </button>
    </transition>
  </div>
</template>

<script>
export default {
  name: "Hotspot",
  data() {
    return {
    };
  },
  props: {
    id: { default: null },
    show: { default: false },
    imageUrl: { default: null },
    positionX: { default: null },
    positionY: { default: null },
    offsetX: { default: "-50%" },
    offsetY: { default: "-50%" },
    sizeX: { default: "100%" },
    sizeY: { default: "100%" },
    labelText: { default: null },
    textAlignX: { default: "center" },
    textAlignY: { default: "center" },
    textOffsetX: { default: "0px" },
    textOffsetY: { default: "0px" },
    zIndex: { default: "1" },
    isButton: { default: false },
  },
  methods: {
    onSelect: function (event) {
      pc.app.fire("hotspot:release", this.id);
      event.stopPropagation();
      event.preventDefault();
    },
    onTouchStart: function (event) {
      pc.app.fire("hotspot:release", this.id);
      event.stopPropagation();
      event.preventDefault();
    }
  },
  mounted() {},
  computed: {
    hsPosition() {
      return {
        "pointer-events": "none",
        position: `absolute`,
        "z-index": `${this.zIndex}`,
        left: `${this.positionX}px`,
        top: `${this.positionY}px`,
      };
    },
    btnStyle() {
      return {
        transform: `translate(${this.offsetX ? this.offsetX : "-50%"}, ${
          this.offsetY ? this.offsetY : "-50%"
        })`,
      };
    },
    hsStyle() {
      return {
        width: `${this.sizeX}`,
        height: `${this.sizeY}`,
      };
    },
    textStyle() {
      return {
        left: `calc(50% + ${this.textOffsetX}`,
        top: `calc(50% + ${this.textOffsetY}`,
        transform: `translate(${
          this.textAlignX === "left"
            ? "0%"
            : this.textAlignX === "right"
            ? "-100%"
            : "-50%"
        }, ${
          this.textAlignY === "top"
            ? "0%"
            : this.textAlignY === "bottom"
            ? "-100%"
            : "-50%"
        })`,
      };
    },
  },
};
</script>

<style scoped>
img {
  max-width: 100%;
  max-height: 100%;
  height: auto;
  width: auto;
}

.blend-enter,
.blend-leave-to {
  opacity: 0;
}

.blend-enter-active,
.blend-leave-active {
  transition: 0.5s ease-in-out;
}

.txt {
  position: fixed;
  color: white;
  text-shadow: 0px -0px 5px rgba(0, 0, 0, 0.9), 2px 2px 3px rgba(0, 0, 0, 0.9);
  pointer-events: none;
}

.pc-button {
  border: none;
  outline: none;
  background: rgba(0, 0, 0, 0);
}

.hotspot {
  pointer-events: all;
  transition: 0.3s;
}

.hotspot:hover {
  filter: drop-shadow(0 0 10px #15919a);
  cursor: pointer;
}
</style>