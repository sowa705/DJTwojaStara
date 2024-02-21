<template>
  <Line
      id="my-chart-id"
      :options="chartOptions"
      :data="chartData"
  />
</template>

<script>
import { Line } from 'vue-chartjs'
import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement } from 'chart.js'

ChartJS.register(
    Title,
    Tooltip,
    Legend,
    LineElement,
    LinearScale,
    PointElement,
    CategoryScale
)

export default {
  name: 'PerformanceChart',
  components: { Line },
  data() {
    return {
      chartData: {
        labels: [ 'January', 'February', 'March' ],
        datasets: [ 
          { 
            label: 'CPU usage (%)',
            data: [40, 20, 12],
            backgroundColor: 'rgba(255, 0, 0, 1)',
            borderColor: 'rgba(255, 0, 0, 1)',
            borderWidth: 1
          },
          {
            label: 'Memory usage (%)',
            data: [30, 50, 2],
            backgroundColor: 'rgba(0, 255, 0, 1)',
            borderColor: 'rgba(0, 255, 0, 1)',
            borderWidth: 1
          },
          {
            label: 'Song cache usage (%)',
            data: [50, 10, 26],
            backgroundColor: 'rgba(0, 128, 255, 1)',
            borderColor: 'rgba(0, 128, 255, 1)',
            borderWidth: 1
          }
        ],
      },
      chartOptions: {
        responsive: true
      }
    }
  }
}
</script>