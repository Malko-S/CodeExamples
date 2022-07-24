<template>
  <transition name="slide">
    <div
      v-if="show"
      :style="containerStyle"
      class="container"
      @mousedown.left="onClick"
      @mousemove="onMoveMouse"
      @mouseup.left="stopDrag"
      @mouseleave="stopDrag"
      @mouseout="stopDrag"
      @touchstart="onTouch"
      @touchmove="onMoveTouch"
      @touchend="stopDrag"
      @touchcancel="stopDrag"
    >
      <div v-if="headline" class="text">{{ headline }}</div>
      <div class="progContainer">
        <div v-for="progressDot in progressDots" :key="progressDot.id">
          <ProgressbarDot
            :id="progressDot.id"
            :position="progressDot.position"
            :duration="progressDot.duration"
          ></ProgressbarDot>
        </div>
        <div class="progBar" :style="barStyle"></div>
        <div
          v-show="progressDots"
          class="progHandle"
          :style="handleStyle"
        ></div>
      </div>
    </div>
  </transition>
</template>

<script>
import ProgressbarDot from "./progressbarDot";
export default {
  name: "Progressbar",
  components: { ProgressbarDot },
  data() {
    return {
      show: false,
      isDragging: false,
      progress: 0,
      progressDots: null,
      headline: null,
      offsetBottom: null,
      sizeX: null,
    };
  },
  props: {},
  computed: {
    containerStyle() {
      return {
        bottom: `${this.offsetBottom ? this.offsetBottom : "24px"}`,
        left: `${
          this.sizeX ? "calc((100vw - 27px - " + this.sizeX + ") / 2)" : "40px"
        }`,
        right: `${
          this.sizeX ? "calc((100vw - 27px - " + this.sizeX + ") / 2)" : "40px"
        }`,
      };
    },
    barStyle() {
      return {
        width: `${this.progress * 100}%`,
      };
    },
    handleStyle() {
      return {
        left: `${this.progress * 100}%`,
      };
    },
  },
  mounted() {
    pc.app.on("progressBar:initialize", (data) => {
      this.show = false;
      this.headline = data.headline ? data.headline : null;
      this.progressDots = data.progressDots ? data.progressDots : null;
    });
    pc.app.on("progressBar:setHeadline", (data) => {
      this.headline = data.headline;
    });
    pc.app.on("progressBar:show", (data) => {
      this.show = true;
      this.progress = data.progress ? data.progress : 0;
      this.offsetBottom = data.offsetBottom ?data.offsetBottom : null;
      this.sizeX = data.sizeX ? data.sizeX : null;
    });
    pc.app.on("progressBar:update", (data) => {
      this.progress = data.progress;
    });
    pc.app.on("progressBar:hide", (data) => {
      this.show = false;
    });
  },
  methods: {
    onClick: function (event) {
      this.isDragging = true;
      this.fireRelease(event.offsetX);
    },
    onTouch: function (event) {
      this.isDragging = true;
      this.fireRelease(event.touches[0].clientX - 40);
    },
    onMoveTouch: function (event) {
      if (this.isDragging) {
        this.fireRelease(event.touches[0].clientX - 40);
      }
    },
    onMoveMouse: function (event) {
      if (this.isDragging) {
        this.fireRelease(event.offsetX);
      }
    },
    stopDrag: function (event) {
      this.isDragging = false;
    },
    fireRelease: function (offsetX) {
      pc.app.fire(
        "progressBar:release",
        Math.clamp((offsetX - 13) / (this.$el.clientWidth - 26), 0, 1)
      );
      // console.log(Math.clamp((offsetX - 13) / (this.$el.clientWidth - 26), 0, 1));
      // this.progress = Math.clamp((offsetX - 13) / (this.$el.clientWidth - 26),0,1);
    },
  },
};
</script>

<style scoped>
.container {
  border: solid #01757b 1px;
  background: linear-gradient(
    180deg,
    rgba(0, 32, 34, 0.9) 0%,
    rgba(0, 0, 0, 0.9) 100%
  );
  box-shadow: 0px 0px 10px 5px #15919a3f;
  position: absolute;
  padding: 10px 13px 11px 13px;
  /* width: 50vw; */
  z-index: 100;
}

.text {
  color: white;
  font-size: 14px;
  margin-bottom: 12px;
  font-weight: 400;
  pointer-events: none;
}

.progContainer {
  background: white;
  position: relative;
  height: 1px;
  pointer-events: none;
}

.progBar {
  background-color: #83cc31;
  height: inherit;
  /* transition: 0.1s ease-in-out; */
  pointer-events: none;
  position: absolute;
}

.progHandle {
  height: 9px;
  width: 9px;
  border-radius: 5px;
  transform: translate(-50%, -50%);
  position: absolute;
  background: #83cc31;
  /* transition: 0.1s ease-in-out; */
  pointer-events: none;
}

.slide-enter,
.slide-leave-to {
  opacity: 0;
}

.slide-enter-active,
.slide-leave-active {
  transition: 0.5s ease-in-out;
}
</style>